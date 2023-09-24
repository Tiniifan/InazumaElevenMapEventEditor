using System;
using System.Collections.Generic;

namespace InazumaElevenGoMapEventEditor.InazumaElevenGo.Logic
{
    public class Event
    {
        public string Name;

        public TalkConfig Config;

        public override string ToString()
        {
            return Name;
        }
    }

    public class TalkInfo
    {
        public int ID;
        public int EventStartIndex;
        public int EventCount;

        public TalkInfo(int[] values)
        {
            int valueIndex = 0;
            var fields = typeof(TalkInfo).GetFields();

            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                var fieldType = field.FieldType;

                if (fieldType.IsArray)
                {
                    int arrayLength = ((Array)field.GetValue(this)).Length;
                    Array.Copy(values, valueIndex, (Array)field.GetValue(this), 0, arrayLength);
                    valueIndex += arrayLength;
                }
                else
                {
                    field.SetValue(this, values[valueIndex]);
                    valueIndex++;
                }
            }
        }

        public int[] ToArray()
        {
            List<int> values = new List<int>();
            var fields = typeof(TalkInfo).GetFields();

            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                var fieldType = field.FieldType;

                if (fieldType.IsArray)
                {
                    values.AddRange((int[])field.GetValue(this));
                }
                else
                {
                    values.Add((int)field.GetValue(this));
                }
            }

            return values.ToArray();
        }
    }

    public class TalkConfig
    {
        public int EventType;
        public int EventID;
        public int Unknown1;
        public int Unknown2;

        public TalkConfig(int[] values)
        {
            int valueIndex = 0;
            var fields = typeof(TalkConfig).GetFields();

            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                var fieldType = field.FieldType;

                if (fieldType.IsArray)
                {
                    int arrayLength = ((Array)field.GetValue(this)).Length;
                    Array.Copy(values, valueIndex, (Array)field.GetValue(this), 0, arrayLength);
                    valueIndex += arrayLength;
                }
                else
                {
                    field.SetValue(this, values[valueIndex]);
                    valueIndex++;
                }
            }
        }

        public int[] ToArray()
        {
            List<int> values = new List<int>();
            var fields = typeof(TalkConfig).GetFields();

            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                var fieldType = field.FieldType;

                if (fieldType.IsArray)
                {
                    values.AddRange((int[])field.GetValue(this));
                }
                else
                {
                    values.Add((int)field.GetValue(this));
                }
            }

            return values.ToArray();
        }
    }
}
