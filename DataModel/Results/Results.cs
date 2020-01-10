﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DataModel.Annotations;

namespace DataModel.Results
{
    public class Results : INotifyPropertyChanged
    {
        private float? _kendallCoefficient;
        private List<PartialUtility> _partialUtilityFunctions;


        public Results()
        {
            FinalRanking = new FinalRanking();
            PartialUtilityFunctions = new List<PartialUtility>();
            KendallCoefficient = null;
        }


        public FinalRanking FinalRanking { get; set; }

        public float? KendallCoefficient
        {
            get => _kendallCoefficient;
            set
            {
                if (Nullable.Equals(value, _kendallCoefficient)) return;
                _kendallCoefficient = value;
                OnPropertyChanged();
            }
        }

        public List<PartialUtility> PartialUtilityFunctions
        {
            get => _partialUtilityFunctions;
            set
            {
                _partialUtilityFunctions = value;
                OnPropertyChanged(nameof(PartialUtilityFunctions));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        public void Reset()
        {
            FinalRanking.FinalRankingCollection.Clear();
            PartialUtilityFunctions.Clear();
            KendallCoefficient = 0;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}