using System;

namespace LuckyDraw.Model
{
    internal class AwardConfiguration
    {
        private String label;

        private int number;

        private int order;

        public string Label { get => label; set => label = value; }
        public int Number { get => number; set => number = value; }
        public int Order { get => order; set => order = value; }
    }
}
