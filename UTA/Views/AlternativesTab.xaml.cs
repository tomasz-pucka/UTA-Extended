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

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using DataModel.Input;
using UTA.ViewModels;

namespace UTA.Views
{
    public partial class AlternativesTab : UserControl
    {
        private readonly Style _criterionValueCellStyle;
        private readonly Style _criterionValueHeaderStyle;
        private readonly List<DataGridTextColumn> _criterionValuesColumns;
        private int _lastDataGridSelectedItem = -1;
        private AlternativesTabViewModel _viewmodel;


        public AlternativesTab()
        {
            InitializeComponent();
            _criterionValuesColumns = new List<DataGridTextColumn>();
            _criterionValueHeaderStyle = (Style) FindResource("CenteredDataGridColumnHeader");
            _criterionValueCellStyle = (Style) FindResource("CenteredDataGridCell");

            Loaded += ViewLoaded;
        }

        private void ViewLoaded(object sender, RoutedEventArgs e)
        {
            _viewmodel = (AlternativesTabViewModel) DataContext;

            _viewmodel.Criteria.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName != nameof(_viewmodel.Criteria.CriteriaCollection)) return;
                InitializeCriterionValueColumnsGenerator();
                GenerateCriterionValuesColumns();
            };
            InitializeCriterionValueColumnsGenerator();

            _viewmodel.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == nameof(_viewmodel.NameTextBoxFocusTrigger))
                {
                    NameTextBox.Focus();
                }
                else if (args.PropertyName == nameof(_viewmodel.AlternativeIndexToShow))
                {
                    AlternativesDataGrid.SelectedIndex = _viewmodel.AlternativeIndexToShow;
                    if (AlternativesDataGrid.SelectedItem == null) return;
                    AlternativesDataGrid.ScrollIntoView(AlternativesDataGrid.SelectedItem);
                }
            };

            if (_viewmodel.AlternativeIndexToShow != -1)
            {
                AlternativesDataGrid.SelectedIndex = _viewmodel.AlternativeIndexToShow;
                if (AlternativesDataGrid.SelectedItem != null) AlternativesDataGrid.ScrollIntoView(AlternativesDataGrid.SelectedItem);
            }
            else
            {
                AlternativesDataGrid.SelectedItem = null;
            }

            NameTextBox.Focus();
            GenerateCriterionValuesColumns();
        }

        private void InitializeCriterionValueColumnsGenerator()
        {
            _viewmodel.Criteria.CriteriaCollection.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Reset)
                    GenerateCriterionValuesColumns();
            };
        }

        private void GenerateCriterionValuesColumns()
        {
            foreach (var criterionValuesColumn in _criterionValuesColumns) AlternativesDataGrid.Columns.Remove(criterionValuesColumn);
            _criterionValuesColumns.Clear();

            for (var i = 0; i < _viewmodel.Criteria.CriteriaCollection.Count; i++)
            {
                var criterionValueColumn = new DataGridTextColumn
                {
                    Binding = new Binding
                    {
                        Path = new PropertyPath($"CriteriaValuesList[{i}].Value"),
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    },
                    Header = _viewmodel.Criteria.CriteriaCollection[i].Name,
                    HeaderStyle = _criterionValueHeaderStyle,
                    CellStyle = _criterionValueCellStyle
                };

                _criterionValuesColumns.Add(criterionValueColumn);
                AlternativesDataGrid.Columns.Add(criterionValueColumn);
            }
        }

        private void CriteriaDataGridPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) e.Handled = true;
        }

        private void DataGridRowClicked(object sender, DataGridRowDetailsEventArgs e)
        {
            var alternativesList = (IList<Alternative>) ((DataGrid) sender).ItemsSource;
            var selectedAlternative = (Alternative) e.Row.Item;
            _lastDataGridSelectedItem = alternativesList.IndexOf(selectedAlternative);
        }

        private void TabUnloaded(object sender, RoutedEventArgs e)
        {
            if (_lastDataGridSelectedItem < _viewmodel.Alternatives.AlternativesCollection.Count)
                _viewmodel.AlternativeIndexToShow = _lastDataGridSelectedItem;
        }

        // focus when alternative is added
        private void NameTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (((TextBox) sender).Text == "") NameTextBox.Focus();
        }
    }
}