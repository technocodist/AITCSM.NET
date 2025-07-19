using AITCSM.NET.Data.Entities.Abstractions;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AITCSM.NET.Data.Entities;

public class FreeFallWithAirResistance : EntityBase
{
    public double TimeStep { get; set; }
    public int StepCount { get; set; }
    public double VelocityEffectivePower { get; set; }
    public double InitialVelocity { get; set; }
    public double InitialHeight { get; set; }
    public double Mass { get; set; }
    public double Gravity { get; set; }
    public double DragCoeffecient { get; set; }
    public int ResultPerSteps { get; set; }

    public virtual List<FreeFallWithAirResistanceStepResult> Results { get; protected set; } = [];
}

public class FreeFallWithAirResistanceStepResult : EntityBase
{
    public int FreeFallWithAirResistanceId { get; set; }
    public int StepNumber { get; set; }

    [JsonIgnore]
    public byte[] TimeStepsBytes
    {
        get
        {
            using MemoryStream stream = new();
            using (BinaryWriter writer = new(stream))
            {
                writer.Write(TimeSteps.Length);
                foreach (double d in TimeSteps)
                {
                    writer.Write(d);
                }
            }
            return stream.ToArray();
        }
        set
        {
            if (value == null)
            {
                TimeSteps = [];
                return;
            }
            using MemoryStream stream = new(value);
            using BinaryReader reader = new(stream);
            int length = reader.ReadInt32();
            TimeSteps = new double[length];
            for (int i = 0; i < length; i++)
            {
                TimeSteps[i] = reader.ReadDouble();
            }
        }
    }

    [NotMapped]
    public double[] TimeSteps { get; set; } = [];


    [JsonIgnore]
    public byte[] VelocitiesBytes
    {
        get
        {
            using MemoryStream stream = new();
            using (BinaryWriter writer = new(stream))
            {
                writer.Write(Velocities.Length);
                foreach (double d in Velocities)
                {
                    writer.Write(d);
                }
            }
            return stream.ToArray();
        }
        set
        {
            if (value == null)
            {
                Velocities = [];
                return;
            }
            using MemoryStream stream = new(value);
            using BinaryReader reader = new(stream);
            int length = reader.ReadInt32();
            Velocities = new double[length];
            for (int i = 0; i < length; i++)
            {
                Velocities[i] = reader.ReadDouble();
            }
        }
    }

    [NotMapped]
    public double[] Velocities { get; set; } = [];


    [JsonIgnore]
    public byte[] PositionsBytes
    {
        get
        {
            using MemoryStream stream = new();
            using (BinaryWriter writer = new(stream))
            {
                writer.Write(Positions.Length);
                foreach (double d in Positions)
                {
                    writer.Write(d);
                }
            }
            return stream.ToArray();
        }
        set
        {
            if (value == null)
            {
                Positions = [];
                return;
            }
            using MemoryStream stream = new(value);
            using BinaryReader reader = new(stream);
            int length = reader.ReadInt32();
            Positions = new double[length];
            for (int i = 0; i < length; i++)
            {
                Positions[i] = reader.ReadDouble();
            }
        }
    }


    [NotMapped]
    public double[] Positions { get; set; } = [];


    [JsonIgnore]
    public byte[] DragForcesBytes
    {
        get
        {
            using MemoryStream stream = new();
            using (BinaryWriter writer = new(stream))
            {
                writer.Write(DragForces.Length);
                foreach (double d in DragForces)
                {
                    writer.Write(d);
                }
            }
            return stream.ToArray();
        }
        set
        {
            if (value == null)
            {
                DragForces = [];
                return;
            }
            using MemoryStream stream = new(value);
            using BinaryReader reader = new(stream);
            int length = reader.ReadInt32();
            DragForces = new double[length];
            for (int i = 0; i < length; i++)
            {
                DragForces[i] = reader.ReadDouble();
            }
        }
    }


    [NotMapped]
    public double[] DragForces { get; set; } = [];


    [JsonIgnore]
    public byte[] NetForcesBytes
    {
        get
        {
            using MemoryStream stream = new();
            using (BinaryWriter writer = new(stream))
            {
                writer.Write(NetForces.Length);
                foreach (double d in NetForces)
                {
                    writer.Write(d);
                }
            }
            return stream.ToArray();
        }
        set
        {
            if (value == null)
            {
                NetForces = [];
                return;
            }
            using MemoryStream stream = new(value);
            using BinaryReader reader = new(stream);
            int length = reader.ReadInt32();
            NetForces = new double[length];
            for (int i = 0; i < length; i++)
            {
                NetForces[i] = reader.ReadDouble();
            }
        }
    }

    [NotMapped]
    public double[] NetForces { get; set; } = [];
}