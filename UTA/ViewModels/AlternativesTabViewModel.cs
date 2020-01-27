﻿// Copyright © 2020 Tomasz Pućka, Piotr Hełminiak, Marcin Rochowiak, Jakub Wąsik

// This file is part of UTA Extended.

// UTA Extended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.

// UTA Extended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with UTA Extended.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using UTA.Annotations;
using DataModel.Input;
using DataModel.PropertyChangedExtended;
using UTA.Helpers;
using UTA.Models;
using UTA.Models.Tab;

namespace UTA.ViewModels
{
    public class AlternativesTabViewModel : Tab, INotifyPropertyChanged
    {
        private int _alternativeIndexToShow = -1;
        private bool _nameTextBoxFocusTrigger;
        private Alternative _newAlternative;


        public AlternativesTabViewModel(Criteria criteria, Alternatives alternatives)
        {
            Name = "Alternatives";
            Criteria = criteria;
            Alternatives = alternatives;

            Criteria.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != nameof(Criteria.CriteriaCollection)) return;
                InitializeNewAlternative();
                InitializeNewAlternativeCriterionValuesUpdaterWatcher();
            };

            Alternatives.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != nameof(Alternatives.AlternativesCollection)) return;
                InitializeAlternativeIndexToShowWatcher();
            };

            InitializeNewAlternative();
            InitializeNewAlternativeCriterionValuesUpdaterWatcher();
            InitializeAlternativeIndexToShowWatcher();

            RemoveAlternativeCommand =
                new RelayCommand(alternative => Alternatives.AlternativesCollection.Remove((Alternative) alternative));
            AddAlternativeCommand = new RelayCommand(_ =>
            {
                NewAlternative.Name = NewAlternative.Name.Trim(' ');
                Alternatives.AlternativesCollection.Add(NewAlternative);
                InitializeNewAlternative();
            }, bindingGroup => !((BindingGroup) bindingGroup).HasValidationError && NewAlternative.Name != "" &&
                               NewAlternative.CriteriaValuesList.All(criterionValue => criterionValue.Value != null));
        }


        public Criteria Criteria { get; }
        public Alternatives Alternatives { get; }
        public RelayCommand RemoveAlternativeCommand { get; }
        public RelayCommand AddAlternativeCommand { get; }

        public Alternative NewAlternative
        {
            get => _newAlternative;
            set
            {
                _newAlternative = value;
                OnPropertyChanged(nameof(NewAlternative));
            }
        }

        public bool NameTextBoxFocusTrigger
        {
            get => _nameTextBoxFocusTrigger;
            set
            {
                if (value == _nameTextBoxFocusTrigger) return;
                _nameTextBoxFocusTrigger = value;
                OnPropertyChanged(nameof(NameTextBoxFocusTrigger));
            }
        }

        public int AlternativeIndexToShow
        {
            get => _alternativeIndexToShow;
            set
            {
                _alternativeIndexToShow = value;
                OnPropertyChanged(nameof(AlternativeIndexToShow));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        private void InitializeAlternativeIndexToShowWatcher()
        {
            if (Alternatives.AlternativesCollection.Count == 0) AlternativeIndexToShow = -1;

            Alternatives.AlternativesCollection.CollectionChanged += (sender, args) =>
            {
                if (Alternatives.AlternativesCollection.Count == 0) AlternativeIndexToShow = -1;
            };
        }

        public void InitializeNewAlternativeCriterionValuesUpdaterWatcher()
        {
            foreach (var criterion in Criteria.CriteriaCollection)
                AddCriterionNamePropertyChangedHandler(criterion);

            Criteria.CriteriaCollection.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Add)
                {
                    var addedCriterion = (Criterion) args.NewItems[0];
                    NewAlternative.AddCriterionValue(new CriterionValue(addedCriterion.Name, null));
                    AddCriterionNamePropertyChangedHandler(addedCriterion);
                }
                else if (args.Action == NotifyCollectionChangedAction.Remove)
                {
                    NewAlternative.RemoveCriterionValue(((Criterion) args.OldItems[0]).Name);
                }
                else if (args.Action == NotifyCollectionChangedAction.Reset)
                {
                    InitializeNewAlternative();
                }
            };
        }

        private void AddCriterionNamePropertyChangedHandler(Criterion criterion)
        {
            criterion.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName != nameof(criterion.Name)) return;
                var extendedArgs = (PropertyChangedExtendedEventArgs<string>) e;
                var criterionValueToUpdate =
                    NewAlternative.CriteriaValuesList.First(criterionValue => criterionValue.Name == extendedArgs.OldValue);
                criterionValueToUpdate.Name = extendedArgs.NewValue;
            };
        }

        private void InitializeNewAlternative()
        {
            NewAlternative = new Alternative("", Criteria.CriteriaCollection);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}