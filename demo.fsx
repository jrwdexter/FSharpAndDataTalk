#load "packages\FsLab\Fslab.fsx"
open FSharp
open FSharp.Core
open Deedle
open FSharp.Data
open FSharp.Data.Runtime
let missedVotesUrl = "http://api.nytimes.com/svc/politics/v3/us/legislative/congress/113/house/votes/missed.json"
let apikey = ""

// CSV Location
[<Literal>]
let complaintsCsv = __SOURCE_DIRECTORY__ + "/slides/complaints.csv"

// CSV
type csv = FSharp.Data.CsvProvider<complaintsCsv>
let complaints = csv.Load(complaintsCsv)
complaints.Rows
|> Seq.groupBy (fun c -> c.``Date received``.DayOfWeek)
|> Seq.sortBy fst
|> Seq.map (fun g -> System.Enum.GetName(typeof<System.DayOfWeek>,fst g), snd g |> Seq.length)
|> XPlot.GoogleCharts.Chart.Column

// Using Json provider

type JsonContext = FSharp.Data.JsonProvider< """{
  "status": "OK",
  "copyright": "Copyright (c) 2016 The New York Times Company",
  "results": [{
      "congress": "113",
      "chamber": "House",
      "num_results": "1",
      "offset": "0",
      "members": [{
          "id": "M00000",
          "name": "John Doe",
          "party": "D",
          "state": "NY",
          "district": "1",
          "total_votes": "0",
          "missed_votes": "0",
          "missed_votes_pct": "0.0",
          "rank": "1",
          "notes": "Other notes"
        }]}]}""">

let missedVotes =
  JsonContext.Load(sprintf "%s?api-key=%s" missedVotesUrl apikey)

let congressmen =
  missedVotes.Results
  |> Seq.collect (fun r -> r.Members)

let topMissingCongressman =
  congressmen
  |> Seq.sortBy (fun m -> try m.MissedVotesPct with | ex -> 0.0m)
  |> Seq.rev
  |> Seq.head

// Using the world bank provider and data frame

let dataContext = FSharp.Data.WorldBankData.GetDataContext()
let highTechExports =
  dataContext.Countries
             .``United States``
             .Indicators
             .``High-technology exports (current US$)``

// Continuing with Deedle from above

let highTechExportData =
  WorldBankData.GetDataContext()
               .Countries
               .``United States``
               .Indicators
               .``High-technology exports (current US$)``

let highTechFrame =
  highTechExportData
  |> Frame.ofRecords
  |> Frame.indexRowsInt "Item1"
  |> Frame.mapColKeys (fun _ -> "High Tech Exports")

// Applying stats

let stats =
    [
        "Min" => Stats.min highTechFrame
        "Max" => Stats.max highTechFrame
        "Average" => Stats.mean highTechFrame
        "Standard Deviation" => Stats.stdDev highTechFrame
    ]
let observations =
    highTechFrame?``High Tech Exports``
    |> Series.observations
    |> Seq.map (fun (k,v) -> float k, float v)

let regression =
    observations
    |> MathNet.Numerics.LinearRegression.SimpleRegression.Fit

let regressedValues = highTechFrame.RowKeys |> Seq.map (fun rk -> float rk, fst regression + (snd regression) * float rk)
open MathNet.Numerics

let rsquared = GoodnessOfFit.RSquared(regressedValues |> Seq.map snd,
                                      observations    |> Seq.map snd)

// Combining data and creating new features

let exportFrame =
  WorldBankData.GetDataContext()
               .Countries
               .``United States``
               .Indicators
               .``Exports of goods and services (current US$)``
  |> Frame.ofRecords
  |> Frame.indexRowsInt "Item1"
  |> Frame.mapColKeys (fun _ -> "Total Exports")

exportFrame?``High Tech Exports`` <- highTechFrame?``High Tech Exports``
exportFrame?``Percentage of High Tech Exports`` <-
  exportFrame?``High Tech Exports`` / exportFrame?``Total Exports``
exportFrame

// Utilizing R

open RProvider.``base``
open RProvider
let rFrame = R.as_data_frame(exportFrame)
let rFrameSummary = (R.summary rFrame)

(*** hide ***)
let rFrameSummaryIntoFSharp : Frame<string, string> = rFrameSummary.GetValue()
let rFrameText =
  rFrameSummaryIntoFSharp
  |> Frame.mapRowValues (fun r -> sprintf "%A %A" <|| (r.Get("Var2"),r.Get("Freq")))
  |> Series.values
  |> Seq.map (fun s -> s.Replace("\"", ""))
  |> Seq.toList

// Charting

open XPlot.GoogleCharts

congressmen
|> Seq.filter (fun c -> try c.MissedVotes >= 0 with _ -> false)
|> Seq.groupBy (fun c -> c.Party)
|> Seq.map (fun g -> fst g, (snd g) |> Seq.sumBy (fun c -> c.MissedVotes))
|> Chart.Pie
|> Chart.WithLegend true

// Scatter
exportFrame?``Total Exports``
|> Series.observations
|> Chart.Scatter
|> Chart.WithTitle "Total US Exports over Time"

