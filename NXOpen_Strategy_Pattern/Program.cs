// Kata 01 — Strategy
// Origem: IMachiningStrategy + MachiningContext (Form1_Load registra ~25 estrategias)
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kata01_Strategy
{
    public enum FeatureKind { ThroughHole, BlindHole, Thread, Counterbore, Pocket, PlanarFace }

    public class Feature
    {
        public string Name; public FeatureKind Kind; public double Diameter; public double Depth;
        public override string ToString() { return string.Format("{0} ({1}, Ø{2:F1} x {3:F1})", Name, Kind, Diameter, Depth); }
    }

    // O contrato. Quem implementa decide se sabe tratar a feature (CanHandle)
    // e com que prioridade — assim duas estrategias podem disputar a mesma
    // feature e a mais especifica vence.
    public interface IMachiningStrategy
    {
        string FeatureName { get; }
        int Priority { get; }                       // maior = mais especifica
        bool CanHandle(Feature f);
        IEnumerable<string> Execute(Feature f);     // devolve as operacoes que criaria
    }

    public class M8TapStrategy : IMachiningStrategy
    {
        public string FeatureName { get { return "M8 Tap"; } }
        public int Priority { get { return 20; } }
        public bool CanHandle(Feature f) { return f.Kind == FeatureKind.Thread && Math.Abs(f.Diameter - 8) < 0.01; }
        public IEnumerable<string> Execute(Feature f)
        {
            yield return "CENTER_DRILL " + f.Name;
            yield return "DRILL_6.8 " + f.Name;
            yield return "TAP_M8 " + f.Name;
        }
    }

    public class GenericTapStrategy : IMachiningStrategy
    {
        public string FeatureName { get { return "Generic Tap"; } }
        public int Priority { get { return 5; } }
        public bool CanHandle(Feature f) { return f.Kind == FeatureKind.Thread; }
        public IEnumerable<string> Execute(Feature f)
        {
            double drill = Math.Round(f.Diameter * 0.85, 1);
            yield return "CENTER_DRILL " + f.Name;
            yield return "DRILL_" + drill + " " + f.Name;
            yield return "TAP_M" + f.Diameter + " " + f.Name;
        }
    }

    public class ThroughPeckDrillStrategy : IMachiningStrategy
    {
        public string FeatureName { get { return "Through Peck Drill"; } }
        public int Priority { get { return 10; } }
        public bool CanHandle(Feature f) { return f.Kind == FeatureKind.ThroughHole; }
        public IEnumerable<string> Execute(Feature f) { yield return "PECK_DRILL_" + f.Diameter + " " + f.Name; }
    }

    public class DeepHoleStrategy : IMachiningStrategy
    {
        public string FeatureName { get { return "Deep Hole (gun drill)"; } }
        public int Priority { get { return 15; } }
        public bool CanHandle(Feature f) { return f.Kind == FeatureKind.ThroughHole && f.Depth / f.Diameter > 8; }
        public IEnumerable<string> Execute(Feature f) { yield return "PILOT_" + f.Diameter + " " + f.Name; yield return "GUN_DRILL " + f.Name; }
    }

    public class OpenPocketStrategy : IMachiningStrategy
    {
        public string FeatureName { get { return "Open Pocket"; } }
        public int Priority { get { return 10; } }
        public bool CanHandle(Feature f) { return f.Kind == FeatureKind.Pocket; }
        public IEnumerable<string> Execute(Feature f) { yield return "POCKET_ROUGH " + f.Name; yield return "POCKET_FLOOR_FINISH " + f.Name; yield return "POCKET_WALL_FINISH " + f.Name; }
    }

    // O contexto: nao conhece nenhuma estrategia concreta.
    public class MachiningContext
    {
        private readonly Dictionary<string, IMachiningStrategy> _byName = new Dictionary<string, IMachiningStrategy>(StringComparer.OrdinalIgnoreCase);

        public void Register(IMachiningStrategy s) { _byName[s.FeatureName] = s; }
        public IEnumerable<string> Names { get { return _byName.Keys; } }

        // Execucao por nome (o que o combo do Form1 faz)
        public IEnumerable<string> Execute(string featureName, Feature f)
        {
            IMachiningStrategy s;
            if (!_byName.TryGetValue(featureName, out s)) throw new KeyNotFoundException("No strategy named " + featureName);
            return s.Execute(f);
        }

        // Escolha automatica: a estrategia de maior prioridade que aceita a feature
        public IMachiningStrategy Resolve(Feature f)
        {
            return _byName.Values.Where(s => s.CanHandle(f)).OrderByDescending(s => s.Priority).FirstOrDefault();
        }
    }

    public static class Program
    {
        public static void Main()
        {
            var ctx = new MachiningContext();
            ctx.Register(new M8TapStrategy());
            ctx.Register(new GenericTapStrategy());
            ctx.Register(new ThroughPeckDrillStrategy());
            ctx.Register(new DeepHoleStrategy());
            ctx.Register(new OpenPocketStrategy());

            var features = new[]
            {
                new Feature { Name = "H1", Kind = FeatureKind.Thread, Diameter = 8, Depth = 16 },
                new Feature { Name = "H2", Kind = FeatureKind.Thread, Diameter = 12, Depth = 24 },
                new Feature { Name = "H3", Kind = FeatureKind.ThroughHole, Diameter = 10, Depth = 30 },
                new Feature { Name = "H4", Kind = FeatureKind.ThroughHole, Diameter = 6, Depth = 90 },   // profundo
                new Feature { Name = "P1", Kind = FeatureKind.Pocket, Diameter = 0, Depth = 12 },
                new Feature { Name = "F1", Kind = FeatureKind.PlanarFace, Diameter = 0, Depth = 0 },     // ninguem trata
            };

            foreach (Feature f in features)
            {
                IMachiningStrategy s = ctx.Resolve(f);
                Console.WriteLine(f);
                if (s == null) { Console.WriteLine("   -> no strategy\n"); continue; }
                Console.WriteLine("   -> " + s.FeatureName + " (priority " + s.Priority + ")");
                foreach (string op in s.Execute(f)) Console.WriteLine("      " + op);
                Console.WriteLine();
            }

            // EXERCICIO: adicione PlanarFaceStrategy sem tocar em MachiningContext
            // nem neste Main alem da linha de Register. Depois troque a prioridade
            // do M8 para 1 e veja o Generic vencer — o contexto nao muda.
        }
    }
}
