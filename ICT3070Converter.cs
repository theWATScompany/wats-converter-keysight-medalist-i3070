using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Virinco.WATS.Integration.TextConverter;
using Virinco.WATS.Interface;

namespace Virinco.WATS.Converter.Keysight
{
    public class MedalistI3070Converter : TextConverterBase
    {
        Dictionary<string, string> parameters;
        public MedalistI3070Converter() : this(new Dictionary<string, string>
        {
            {"partRevision","1.0" },
            {"operationTypeCode","30" },
            {"validationMode","AutoTruncate" }
        })
        { }

        //public Dictionary<string, string> ConverterParameters => parameters;

        public new void CleanUp()
        {
        }

        string group = "";
        string compRef = "";
        string reportText = "";
        NumericLimitStep multiNumericStep = null;

        protected override bool ProcessMatchedLine(SearchFields.SearchMatch match, ref ReportReadState readState)
        {
            if (match == null)
            {
                //Console.WriteLine($"SN: {currentUUT.SerialNumber} PN:{currentUUT.PartNumber} Test setup: {currentUUT.MiscInfo.Where(m=>m.Description=="Test Setup").First().DataString} Seq={currentUUT.SequenceName}");
                if (!string.IsNullOrEmpty(currentUUT.SerialNumber))
                {
                    if (!string.IsNullOrEmpty(reportText))
                        currentUUT.Comment = reportText;
                    //currentUUT.AddMiscUUTInfo("FileName", apiRef.ConversionSource.SourceFile.Name);
                    apiRef.Submit(SubmitMethod.Online, currentUUT);
                }
                return true;
            }
            switch (match.matchField.fieldName)
            {
                case "Block1":
                case "Block2":
                    compRef = (string)match.GetSubField("CompRef");
                    multiNumericStep = null;
                    reportText = "";
                    break;
                case "TestLIM2":
                case "TestLIM3":
                    if ((string)match.GetSubField("Group") != group)
                    {
                        group = (string)match.GetSubField("Group");
                        currentSequence = currentUUT.GetRootSequenceCall().AddSequenceCall(group);
                    }
                    currentSequence.AddNumericLimitStep(compRef).AddTest((double)match.GetSubField("Meas"), CompOperatorType.GELE, (double)match.GetSubField("LowLim"), (double)match.GetSubField("HighLim"), "");
                    break;
                case "TestLIM2Multi":
                    if ((string)match.GetSubField("Group") != group)
                    {
                        group = (string)match.GetSubField("Group");
                        currentSequence = currentUUT.GetRootSequenceCall().AddSequenceCall(group);
                    }
                    if (multiNumericStep == null)
                        multiNumericStep = currentSequence.AddNumericLimitStep(compRef);
                    multiNumericStep.AddMultipleTest((double)match.GetSubField("Meas"), CompOperatorType.GELE, (double)match.GetSubField("LowLim"), (double)match.GetSubField("HighLim"), "", (string)match.GetSubField("MeasName"));
                    break;
                case "TestPassFail":
                    if ((string)match.GetSubField("Group") != group)
                    {
                        group = (string)match.GetSubField("Group");
                        currentSequence = currentUUT.GetRootSequenceCall().AddSequenceCall(group);
                    }
                    currentSequence.AddPassFailStep((string)match.GetSubField("CompRef")).AddTest((double)match.GetSubField("Res1") == 0);
                    break;
                case "Report":
                    if (!string.IsNullOrEmpty((string)match.GetSubField("RepTxt")))
                        reportText += (string)match.GetSubField("RepTxt") + "\r\n";
                    break;
                default:
                    break;
            }
            return true;
        }

        public MedalistI3070Converter(IDictionary<string, string> args) : base(args)
        {
            SearchFields.RegExpSearchField regExpSearchField = searchFields.AddRegExpField(UUTField.UseSubFields, ReportReadState.InHeader, @"{@BATCH[|][^|]*[|][^|]*[|][^|]*[|][^|]*[|][^|]*[|][^|]*[|][^|]*[|][^|]*[|](?<Station>[^|]*)[|](?<PartNumber>[^|]*)[|](?<Revision>[^|]*)", null, typeof(string));
            regExpSearchField.AddSubField("Station", typeof(string), null, UUTField.StationName);
            regExpSearchField.AddSubField("PartNumber", typeof(string), null, UUTField.PartNumber);
            regExpSearchField.AddSubField("Revision", typeof(string), null, UUTField.PartRevisionNumber);

            regExpSearchField = searchFields.AddRegExpField(UUTField.UseSubFields, ReportReadState.InHeader, @"{@BTEST[|](?<Serialnumber>[^|]*)[|][^|]*[|][^|]*[|][^|]*[|][^|]*[|][^|]*[|][^|]*[|][^|]*[|][^|]*[|](?<StartTime>[^|]*)", null, typeof(string), ReportReadState.InTest);
            regExpSearchField.AddSubField("Serialnumber", typeof(string), null, UUTField.SerialNumber);
            regExpSearchField.AddSubField("StartTime", typeof(DateTime), "yyMMddHHmmss", UUTField.StartDateTime);

            regExpSearchField = searchFields.AddRegExpField(UUTField.Status, ReportReadState.InTest, @".*{@STATUS[|](?<UUTStatus>.+)}", null, typeof(UUTStatusType));

            regExpSearchField = searchFields.AddRegExpField("Block1", ReportReadState.InTest, @"{@BLOCK[|](?<Nest>.*)%(?<CompRef>[^|]*)[|][^|]*", null, typeof(string));
            regExpSearchField.AddSubField("CompRef", typeof(string), null);

            regExpSearchField = searchFields.AddRegExpField("Block2", ReportReadState.InTest, @"{@BLOCK[|](?<CompRef>[^|]*)[|][^|]*", null, typeof(string));
            regExpSearchField.AddSubField("CompRef", typeof(string), null);

            regExpSearchField = searchFields.AddRegExpField("TestLIM2", ReportReadState.InTest, @"{@(?<Group>[^|]+)[^|]*[|][^|]*[|](?<Meas>[0-9+-E.]+){@LIM2[|](?<HighLim>[0-9+-E.]+)[|](?<LowLim>[0-9+-E.]+)}}", null, typeof(string));
            regExpSearchField.AddSubField("Group", typeof(string), null);
            regExpSearchField.AddSubField("Meas", typeof(double), null);
            regExpSearchField.AddSubField("HighLim", typeof(double), null);
            regExpSearchField.AddSubField("LowLim", typeof(double), null);

            regExpSearchField = searchFields.AddRegExpField("TestLIM2Multi", ReportReadState.InTest, @"{@(?<Group>[^|]+)[|][^|]*[|](?<Meas>[0-9+-E.]+)[|](?<MeasName>[^}]+){@LIM2[|](?<HighLim>[0-9+-E.]+)[|](?<LowLim>[0-9+-E.]+)}}", null, typeof(string));
            regExpSearchField.AddSubField("Group", typeof(string), null);
            regExpSearchField.AddSubField("Meas", typeof(double), null);
            regExpSearchField.AddSubField("MeasName", typeof(string), null);
            regExpSearchField.AddSubField("HighLim", typeof(double), null);
            regExpSearchField.AddSubField("LowLim", typeof(double), null);

            regExpSearchField = searchFields.AddRegExpField("TestLIM3", ReportReadState.InTest, @"{@(?<Group>[^|]+)[^|]*[|][^|]*[|](?<Meas>[0-9+-E.]+){@LIM3[|](?<Nominal>[0-9+-E.]+)[|](?<HighLim>[0-9+-E.]+)[|](?<LowLim>[0-9+-E.]+)}}", null, typeof(string));
            regExpSearchField.AddSubField("Group", typeof(string), null);
            regExpSearchField.AddSubField("Meas", typeof(double), null);
            regExpSearchField.AddSubField("Nominal", typeof(double), null);
            regExpSearchField.AddSubField("HighLim", typeof(double), null);
            regExpSearchField.AddSubField("LowLim", typeof(double), null);

            regExpSearchField = searchFields.AddRegExpField("TestPassFail", ReportReadState.InTest, @"{@(?<Group>[^|]+)[|](?<Nest>.*)%(?<CompRef>[^|]*)[|](?<Res1>\d+)[|](?<Res2>\d+)[|](?<Res3>\d+)}", null, typeof(string));
            regExpSearchField.AddSubField("Group", typeof(string), null);
            regExpSearchField.AddSubField("CompRef", typeof(string), null);
            regExpSearchField.AddSubField("Res1", typeof(double), null);
            regExpSearchField.AddSubField("Res2", typeof(double), null);
            regExpSearchField.AddSubField("Res3", typeof(double), null);

            regExpSearchField = searchFields.AddRegExpField("Report", ReportReadState.InTest, @"{@RPT[|](?<RepTxt>.*)}", null, typeof(string));
            regExpSearchField.AddSubField("RepTxt", typeof(string), null);



        }
    }
}
