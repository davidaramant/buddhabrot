using System.IO;
using System.Linq;
using Buddhabrot.IterationKernels;
using Buddhabrot.Utility;
using CsvHelper;

namespace Buddhabrot.EdgeSpans
{
    public static class BorderPointsLocator
    {
        public static void LocatePoints(string edgeSpansPath, string outputPath, int pointsToCalculate)
        {
            using (var edgeSpans = EdgeSpanStream.Load(edgeSpansPath))
            using (var timedOperation = TimedOperation.Start("edge spans", totalWork: pointsToCalculate > 0 ? pointsToCalculate : edgeSpansPath.Length))
            using (var outputFile = File.OpenWrite(outputPath))
            using (var textWriter = new StreamWriter(outputFile))
            using (var csv = new CsvWriter(textWriter))
            {
                var sequence =
                    edgeSpans.Take((int)timedOperation.TotalWork).
                    AsParallel().
                    Select(result =>
                    {
                        var borderPoint = result.Span.FindBoundaryPoint(Constant.IterationRange.Max);
                        var escapeTime = ScalarDoubleKernel.FindEscapeTime(borderPoint, Constant.IterationRange.Max);
                        timedOperation.AddWorkDone(1);
                        return
                        (
                            Location: result.Location,
                            Span: result.Span,
                            Point: borderPoint,
                            EscapeTime: escapeTime
                        );
                    });

                csv.WriteField("Location");
                csv.WriteField("Span");
                csv.WriteField("Border Point");
                csv.WriteField("Escape Time");
                csv.NextRecord();

                foreach (var result in sequence)
                {
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
