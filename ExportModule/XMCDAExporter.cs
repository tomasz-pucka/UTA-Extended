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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using DataModel.Input;
using DataModel.Results;

namespace ExportModule
{
    public class XMCDAExporter
    {
        private readonly List<Alternative> alternativeList;
        private readonly List<Criterion> criterionList;
        private readonly string outputDirectory;
        public bool OverwriteFile;
        private readonly Results results;
        private XmlTextWriter xmcdaWriter;
        private readonly bool _preserveKendalCoefficient;

        public XMCDAExporter(string outputDirectory,
            List<Criterion> criterionList,
            List<Alternative> alternativeList,
            Results results, 
            bool preserveKendallCoefficient)
        {
            OverwriteFile = false;
            this.outputDirectory = outputDirectory;
            this.criterionList = criterionList;
            this.alternativeList = alternativeList;
            this.results = results;
            _preserveKendalCoefficient = preserveKendallCoefficient;
        }

        public XMCDAExporter(string outputDirectory,
            List<Criterion> criterionList,
            List<Alternative> alternativeList,
            bool preserveKendallCoefficient)
        {
            OverwriteFile = false;
            this.outputDirectory = outputDirectory;
            this.criterionList = criterionList;
            this.alternativeList = alternativeList;
            _preserveKendalCoefficient = preserveKendallCoefficient;
        }

        private void checkIfFileExists(string path)
        {
            if (!OverwriteFile)
                if (File.Exists(path))
                    throw new XmcdaFileExistsException(
                        "File " + Path.GetFileName(path) + " already exists. Would you like to overwrite it?");
        }

        private void checkIfInputFilesExists()
        {
            checkIfFileExists(Path.Combine(outputDirectory, "criteria.xml"));
            checkIfFileExists(Path.Combine(outputDirectory, "alternatives.xml"));
            checkIfFileExists(Path.Combine(outputDirectory, "performance_table.xml"));
            checkIfFileExists(Path.Combine(outputDirectory, "criteria_scales.xml"));
            checkIfFileExists(Path.Combine(outputDirectory, "UTA", "method_parameters.xml"));
            checkIfFileExists(Path.Combine(outputDirectory, "UTA", "alternatives_ranks.xml"));
            checkIfFileExists(Path.Combine(outputDirectory, "UTA", "criteria_segments.xml"));
        }

        private void checkIfResultFilesExists()
        {
            checkIfFileExists(Path.Combine(outputDirectory, "UTA", "value_functions.xml"));
        }

        private void initializeWriter(string filePath)
        {
            xmcdaWriter = new XmlTextWriter(filePath, Encoding.UTF8);
            xmcdaWriter.Formatting = Formatting.Indented;
            xmcdaWriter.Indentation = 2;
            xmcdaWriter.WriteStartDocument(false);
            xmcdaWriter.WriteStartElement("xmcda:XMCDA");
            xmcdaWriter.WriteAttributeString("xmlns:xmcda", "http://www.decision-deck.org/2019/XMCDA-3.1.1");
            xmcdaWriter.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
            xmcdaWriter.WriteAttributeString("xsi:schemaLocation",
                "http://www.decision-deck.org/2019/XMCDA-3.1.1 http://www.decision-deck.org/xmcda/_downloads/XMCDA-3.1.1.xsd");
        }

        private void saveCriterions()
        {
            initializeWriter(Path.Combine(outputDirectory, "criteria.xml"));
            xmcdaWriter.WriteStartElement("criteria");

            foreach (var criterion in criterionList)
            {
                xmcdaWriter.WriteStartElement("criterion");
                xmcdaWriter.WriteAttributeString("id", criterion.ID != null ? criterion.ID : criterion.Name);
                xmcdaWriter.WriteAttributeString("name", criterion.Name);
                xmcdaWriter.WriteStartElement("active");
                xmcdaWriter.WriteString("true");
                xmcdaWriter.WriteEndElement();
                xmcdaWriter.WriteEndElement();
            }

            xmcdaWriter.WriteEndElement();
            xmcdaWriter.WriteEndDocument();
            xmcdaWriter.Close();
        }

        private void saveAlternatives()
        {
            initializeWriter(Path.Combine(outputDirectory, "alternatives.xml"));
            xmcdaWriter.WriteStartElement("alternatives");

            foreach (var alternative in alternativeList)
            {
                xmcdaWriter.WriteStartElement("alternative");
                xmcdaWriter.WriteAttributeString("id", alternative.ID != null ? alternative.ID : alternative.Name);
                xmcdaWriter.WriteAttributeString("name", alternative.Name);
                xmcdaWriter.WriteStartElement("type");
                xmcdaWriter.WriteString("real");
                xmcdaWriter.WriteEndElement();
                xmcdaWriter.WriteStartElement("active");
                xmcdaWriter.WriteString("true");
                xmcdaWriter.WriteEndElement();
                xmcdaWriter.WriteEndElement();
            }

            xmcdaWriter.WriteEndElement();
            xmcdaWriter.WriteEndDocument();
            xmcdaWriter.Close();
        }

        private void saveCriterionScales()
        {
            initializeWriter(Path.Combine(outputDirectory, "criteria_scales.xml"));
            xmcdaWriter.WriteStartElement("criteriaScales");

            foreach (var criterion in criterionList)
            {
                xmcdaWriter.WriteStartElement("criterionScale");
                xmcdaWriter.WriteStartElement("criterionID");
                xmcdaWriter.WriteString(criterion.ID != null ? criterion.ID : criterion.Name);
                xmcdaWriter.WriteEndElement();
                xmcdaWriter.WriteStartElement("scales");
                xmcdaWriter.WriteStartElement("scale");
                xmcdaWriter.WriteStartElement("quantitative");
                xmcdaWriter.WriteStartElement("preferenceDirection");
                xmcdaWriter.WriteString(criterion.CriterionDirection == "Cost" ? "min" : "max");
                xmcdaWriter.WriteEndElement();
                xmcdaWriter.WriteEndElement();
                xmcdaWriter.WriteEndElement();
                xmcdaWriter.WriteEndElement();
                xmcdaWriter.WriteEndElement();
            }

            xmcdaWriter.WriteEndElement();
            xmcdaWriter.WriteEndDocument();
            xmcdaWriter.Close();
        }

        private void savePerformanceTable()
        {
            initializeWriter(Path.Combine(outputDirectory, "performance_table.xml"));
            xmcdaWriter.WriteStartElement("performanceTable");
            xmcdaWriter.WriteAttributeString("mcdaConcept", "REAL");
            foreach (var alternative in alternativeList)
            {
                xmcdaWriter.WriteStartElement("alternativePerformances");
                xmcdaWriter.WriteStartElement("alternativeID");
                xmcdaWriter.WriteString(alternative.ID != null ? alternative.ID : alternative.Name);
                xmcdaWriter.WriteEndElement();

                foreach (var criterionValue in alternative.CriteriaValuesList)
                {
                    xmcdaWriter.WriteStartElement("performance");
                    xmcdaWriter.WriteStartElement("criterionID");
                    var matchingCriterion = criterionList.Find(criterion => criterion.Name == criterionValue.Name);
                    xmcdaWriter.WriteString(matchingCriterion.ID != null ? matchingCriterion.ID : matchingCriterion.Name);
                    xmcdaWriter.WriteEndElement();
                    xmcdaWriter.WriteStartElement("values");
                    xmcdaWriter.WriteStartElement("value");
                    xmcdaWriter.WriteStartElement("real");
                    xmcdaWriter.WriteString(((double) criterionValue.Value).ToString("G", CultureInfo.InvariantCulture));
                    xmcdaWriter.WriteEndElement();
                    xmcdaWriter.WriteEndElement();
                    xmcdaWriter.WriteEndElement();
                    xmcdaWriter.WriteEndElement();
                }

                xmcdaWriter.WriteEndElement();
            }

            xmcdaWriter.WriteEndElement();
            xmcdaWriter.WriteEndDocument();
            xmcdaWriter.Close();
        }

        private void saveReferenceRanking()
        {
            Directory.CreateDirectory(Path.Combine(outputDirectory, "UTA"));
            initializeWriter(Path.Combine(outputDirectory, "UTA", "alternatives_ranks.xml"));
            xmcdaWriter.WriteStartElement("alternativesValues");

            foreach (var alternative in alternativeList)
                if (alternative.ReferenceRank != null)
                {
                    xmcdaWriter.WriteStartElement("alternativeValue");
                    xmcdaWriter.WriteStartElement("alternativeID");
                    xmcdaWriter.WriteString(alternative.ID != null ? alternative.ID : alternative.Name);
                    xmcdaWriter.WriteEndElement();
                    xmcdaWriter.WriteStartElement("value");
                    xmcdaWriter.WriteStartElement("integer");
                    xmcdaWriter.WriteString((alternative.ReferenceRank + 1).ToString());
                    xmcdaWriter.WriteEndElement();
                    xmcdaWriter.WriteEndElement();
                    xmcdaWriter.WriteEndElement();
                }

            xmcdaWriter.WriteEndElement();
            xmcdaWriter.WriteEndDocument();
            xmcdaWriter.Close();
        }

        public void saveCriteriaSegments()
        {
            Directory.CreateDirectory(Path.Combine(outputDirectory, "UTA"));
            initializeWriter(Path.Combine(outputDirectory, "UTA", "criteria_segments.xml"));

            xmcdaWriter.WriteStartElement("criteriaValues");

            foreach (var criterion in criterionList)
            {
                xmcdaWriter.WriteStartElement("criterionValue");
                xmcdaWriter.WriteStartElement("criterionID");
                xmcdaWriter.WriteString(criterion.ID != null ? criterion.ID : criterion.Name);
                xmcdaWriter.WriteEndElement();
                xmcdaWriter.WriteStartElement("value");
                xmcdaWriter.WriteStartElement("integer");
                xmcdaWriter.WriteString(criterion.LinearSegments.ToString());
                xmcdaWriter.WriteEndElement();
                xmcdaWriter.WriteEndElement();
                xmcdaWriter.WriteEndElement();
            }

            xmcdaWriter.WriteEndElement();
            xmcdaWriter.WriteEndDocument();
            xmcdaWriter.Close();
        }

        private void saveValueFunctions()
        {
            Directory.CreateDirectory(Path.Combine(outputDirectory, "UTA"));

            initializeWriter(Path.Combine(outputDirectory, "UTA", "value_functions.xml"));
            xmcdaWriter.WriteStartElement("criteria");
            xmcdaWriter.WriteAttributeString("mcdaConcept", "criteria");

            foreach (var partialUtility in results.PartialUtilityFunctions)
            {
                xmcdaWriter.WriteStartElement("criterion");
                xmcdaWriter.WriteStartElement("criterionID");
                xmcdaWriter.WriteString(partialUtility.Criterion.ID != null ? partialUtility.Criterion.ID : partialUtility.Criterion.Name);
                xmcdaWriter.WriteEndElement();
                xmcdaWriter.WriteStartElement("criterionFunction");
                xmcdaWriter.WriteStartElement("points");
                foreach (var pointValue in partialUtility.PointsValues)
                {
                    xmcdaWriter.WriteStartElement("point");

                    xmcdaWriter.WriteStartElement("abscissa");
                    xmcdaWriter.WriteStartElement("real");
                    xmcdaWriter.WriteString(pointValue.X.ToString("G", CultureInfo.InvariantCulture));
                    xmcdaWriter.WriteEndElement();
                    xmcdaWriter.WriteEndElement();

                    xmcdaWriter.WriteStartElement("ordinate");
                    xmcdaWriter.WriteStartElement("real");
                    xmcdaWriter.WriteString(pointValue.Y.ToString("G", CultureInfo.InvariantCulture));
                    xmcdaWriter.WriteEndElement();
                    xmcdaWriter.WriteEndElement();

                    xmcdaWriter.WriteEndElement();
                }

                xmcdaWriter.WriteEndElement();
                xmcdaWriter.WriteEndElement();
                xmcdaWriter.WriteEndElement();
            }

            xmcdaWriter.WriteEndElement();
            xmcdaWriter.WriteEndDocument();
            xmcdaWriter.Close();
        }

        private void saveKendalPreserveCondition()
        {
            Directory.CreateDirectory(Path.Combine(outputDirectory, "UTA"));
            initializeWriter(Path.Combine(outputDirectory, "UTA", "method_parameters.xml"));

            xmcdaWriter.WriteStartElement("programParameters");
            xmcdaWriter.WriteStartElement("parameter");
            xmcdaWriter.WriteAttributeString("id", "preserve_kendall_coefficient");
            xmcdaWriter.WriteStartElement("values");
            xmcdaWriter.WriteStartElement("value");
            xmcdaWriter.WriteStartElement("boolean");
            xmcdaWriter.WriteString(_preserveKendalCoefficient.ToString().ToLower());
            xmcdaWriter.WriteEndElement();
            xmcdaWriter.WriteEndElement();
            xmcdaWriter.WriteEndElement();
            xmcdaWriter.WriteEndElement();
            xmcdaWriter.WriteEndElement();
            xmcdaWriter.WriteEndDocument();
            xmcdaWriter.Close();
        }

        public void saveInput()
        {
            checkIfInputFilesExists();
            saveCriterions();
            saveAlternatives();
            saveCriterionScales();
            savePerformanceTable();
            saveKendalPreserveCondition();
            saveCriteriaSegments();
            saveReferenceRanking();
        }

        public void saveResults()
        {
            checkIfResultFilesExists();
            if (results != null)
            {
                saveValueFunctions();
            }
            else
            {
                throw new Exception("Results are not available");
            }
        }

        public void saveSession()
        {
            saveInput();
            saveResults();
        }
    }
}