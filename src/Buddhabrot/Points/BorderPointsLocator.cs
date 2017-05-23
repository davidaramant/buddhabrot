using System.IO;
using System.Linq;
using Buddhabrot.Core;
using Buddhabrot.EdgeSpans;
using Buddhabrot.IterationKernels;
using Buddhabrot.Utility;
using CsvHelper;

namespace Buddhabrot.Points
{
    public static class BorderPointsLocator
    {
        public static void CalculateToCsv(string edgeSpansPath, int pointsToCalculate)
        {
            using (var edgeSpans = EdgeSpanStream.Load(edgeSpansPath))
            using (var timedOperation = TimedOperation.Start("edge spans", totalWork: pointsToCalculate > 0 ? pointsToCalculate : edgeSpansPath.Length))
            using (var outputFile = File.OpenWrite(edgeSpansPath + $".{timedOperation.TotalWork}.borderPoints.csv"))
            using (var textWriter = new StreamWriter(outputFile))
            using (var csv = new CsvWriter(textWriter))
            {
                var sequence =
                    edgeSpans.Take((int)timedOperation.TotalWork).
                    Select((logicalSpan, index) => new { LogicalSpan = logicalSpan, Index = index }).
                    AsParallel().
                    Select(result =>
                        {
                            var span = result.LogicalSpan.ToConcrete(edgeSpans.ViewPort);
                            var borderPoint = span.FindBoundaryPoint();
                            var escapeTime = ScalarDoubleKernel.FindEscapeTime(borderPoint, Constant.IterationRange.Max);
                            timedOperation.AddWorkDone(1);
                            return
                            (
                                Index: result.Index,
                                Location: result.LogicalSpan.Location,
                                Span: span,
                                Point: borderPoint,
                                EscapeTime: escapeTime
                            );
                        });

                csv.WriteField("Index");
                csv.WriteField("Location");
                csv.WriteField("Span");
                csv.WriteField("Border Point");
                csv.WriteField("Escape Time");
                csv.NextRecord();

                foreach (var result in sequence)
                {
                    csv.WriteField(result.Index);
                    csv.WriteField(result.Location);
                    csv.WriteField(result.Span);
                    csv.WriteField(result.Point);
                    csv.WriteField(result.EscapeTime);
                    csv.NextRecord();
                }
            }
        }
    }
}
