using System.IO;
using System.Linq;
using Buddhabrot.IterationKernels;
using Buddhabrot.Utility;
using CsvHelper;

namespace Buddhabrot.EdgeSpans
{
    public static class BorderPointsLocator
    {
        public static void LocatePoints(string edgeSpansPath, string outputPath)
        {
            using (var edgeSpans = EdgeSpanStream.Load(edgeSpansPath))
            using (var timedOperation = TimedOperation.Start("edge spans", totalWork: edgeSpans.Count))
            using (var outputFile = File.OpenWrite(outputPath))
            using (var textWriter = new StreamWriter(outputFile))
            using (var csv = new CsvWriter(textWriter))
            {
                var sequence =
                    edgeSpans.
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

                csv.WriteRecords(sequence);
            }
        }
    }
}
