(** - data-background : #293c3d url('/images/watermark.svg') 0 0 repeat *)

(*** hide ***)
#load "..\packages\FsLab\Fslab.fsx"
open FSharp
open FSharp.Core
open Deedle
let missedVotesUrl = "http://api.nytimes.com/svc/politics/v3/us/legislative/congress/113/house/votes/missed.json"
let apikey = ""

[<Literal>]
let complaintsCsv = __SOURCE_DIRECTORY__ + "/complaints.csv"

(**
# F# and Data 101
### Utilizing FsLab to Quickly Understand your Data
#### Jonathan Dexter, Technology Manager of .NET, The Nerdery
![nerdery logo](/images/nerdery_logo.png)

' We're going to talk about F# and data.
' Who I am: Talk about being a developer, my current position, and The Nerdery

***
- data-background : #293c3d url('/images/watermark.svg') 0 0 repeat

### Agenda
* What is this talk about
* Get the data!
* Transform!
* Science!
* ???
* Profit!
*)

(**

***
- data-background : #293c3d url('/images/watermark.svg') 0 0 repeat

### If you came here to hear about
* "Monads"
* "Functors"
* Tail-call optimization
* Immutable design
* Pattern matching

***
- data-background : #293c3d url('/images/watermark.svg') 0 0 repeat

![nope](/images/nope_2.gif)

' Time check: 5 min
' Partially kidding: Part of those items are covered in this talk.
' We will not however cover them in-depth.
' We'll touch on some of these, but that's not the focus.

***
- data-background : #293c3d url('/images/watermark.svg') 0 0 repeat

### What we WILL talk about : FsLab

![fslab](/images/fslab.png)

' FsLab isn't one library. It's a set of tools.
' First, getting FsLab is super easy.

***
- data-background : #293c3d url('/images/watermark.svg') 0 0 repeat

With Paket
```powershell
paket init
paket add nuget fslab
```

---
- data-background : #293c3d url('/images/watermark.svg') 0 0 repeat

With NuGet

```powershell
nuget install fslab -OutputDirectory packages
```

' Or, of course, you could use Visual Studio to install the package.

***
- data-background : #293c3d url('/images/watermark.svg') 0 0 repeat

### The process
(Not limited to FsLab)

```f#
acquire data
|> transform
|> science
|> visualize
```

***
- data-background : #664659 url('images/watermark.svg') 0 0 repeat

## FsLab: Scratching the Surface
5 Libraries

![f# data](/images/fsharp_data.png)
![deedle](/images/deedle.png)
![rtypeprovider](/images/rtypeprovider.png)
![numerics](/images/numerics.png)
![xplot](/images/xplot.png)

---
- data-background : #664659 url('images/watermark.svg') 0 0 repeat

### Sorting the libraries above into categories:

| Library | Acquire | Transform | Science | Display |
|---|---|---|---|---|
| F# Data | **Yep!** | - | - | - |
| Deedle | - | **Yep** | - | - |
| .NET Numerics | - | *Supports* | *Supports* | - |
| R Type Provider | *Partial* | **Yep** | **Yep** | *Partial* |
| XPlot | - | - | - | **Yep** |

***
- data-background : #007b89 url('/images/watermark.svg') 0 0 repeat

### Step one: Acquire

---
- data-background : #007b89 url('/images/watermark.svg') 0 0 repeat

![imadethis](/images/imadethis.jpg)

---
- data-background : #007b89 url('/images/watermark.svg') 0 0 repeat

#### Classic scenario: CSV

Using CSV type provider
*)

type csv = FSharp.Data.CsvProvider<complaintsCsv>
let complaints = csv.Load(complaintsCsv)

(**
Using a data frame
*)
let data = Deedle.Frame.ReadCsv(__SOURCE_DIRECTORY__ + "/data.csv")

(**
' We're going to get this one out of the way because it is so basic.

---
- data-background : #007b89 url('/images/watermark.svg') 0 0 repeat

#### Type Provider Scenario: World bank provider
World bank provider is bundled with F# Data
*)

let dataContext = FSharp.Data.WorldBankData.GetDataContext()
let highTechExports =
  dataContext.Countries
             .``United States``
             .Indicators
             .``High-technology exports (current US$)``

(**
' Now, I promised I wouldn't talk about type providers, but I'm going to briefly.

---
- data-background : #007b89 url('/images/watermark.svg') 0 0 repeat

#### Type Provider Scenario: JSON provider
*)
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
(**
---
- data-background : #007b89 url('/images/watermark.svg') 0 0 repeat

#### Type Provider Scenario: JSON Provider (cont.)
*)

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

(**
---
- data-background : #007b89 url('/images/watermark.svg') 0 0 repeat

#### Type Provider Scenario: JSON Provider (cont.)
*)
(*** include-value: topMissingCongressman ***)


(**
---
- data-background : #007b89 url('/images/watermark.svg') 0 0 repeat

#### Batteries not included: SQL Provider

[SQL Data Connection](https://msdn.microsoft.com/en-us/library/hh362320.aspx) *

[SQL Entity Connectoin](https://msdn.microsoft.com/en-us/library/hh362320.aspx) *

[SQL Client](https://github.com/fsprojects/FSharp.Data.SqlClient)

[SQL Provider](https://github.com/fsprojects/SQLProvider)

---
- data-background : #007b89 url('/images/watermark.svg') 0 0 repeat

#### SQL Data Connection
Type provder for an entire database, MS SQL focused.
```
type dbSchema = SqlDataConnection<"""Data Source=MYSERVER\INSTANCE;
                                     Initial Catalog=MyDatabase;
                                     Integrated Security=SSPI;""">
let db = dbSchema.GetDataContext()
```

---
- data-background : #007b89 url('/images/watermark.svg') 0 0 repeat

#### SQL Entity Connection
Type provder for an entire database, through ADO.NET Entity model.
```
type dbSchema = SqlEntityConnection<"""Data Source=MYSERVER\INSTANCE;
                                       Initial Catalog=MyDatabase;
                                       Integrated Security=SSPI;""">
let db = dbSchema.GetDataContext()
```

---
- data-background : #007b89 url('/images/watermark.svg') 0 0 repeat

#### SQL Client
Type provider for commands, sprocs, and queries

```
use cmd = new SqlCommandProvider<"SELECT Name, Age, Visits
                                  FROM Analytics.Person
                                  WHERE Region = @region",
                                  connectionString>()
```
```
let results = cmd.Execute(region = "USA")
```
---
- data-background : #007b89 url('/images/watermark.svg') 0 0 repeat

#### SQL Provider
Type provider for DB as a whole

*MS SQL, Postgres, SQLite, MySQL, Oracle, MS Access*

```
type sql  = SqlDataProvider<connectionString,
                            Common.DatabaseProviderTypes.MSSQLSERVER>

let ctx = sql.GetDataContext()
```
*)

(**

***
- data-background : #293c3d url('/images/watermark.svg') 0 0 repeat

### Step two: Transform

![transformer](/images/transformer.gif)

---
- data-background : #293c3d url('/images/watermark.svg') 0 0 repeat

#### Deedle: Convert to data frame
*)

open Deedle
open FSharp.Data
open FSharp.Data.Runtime.BaseTypes

(*** hide ***)
Frame.CustomExpanders.Remove(typeof<JsonValue>)
Frame.CustomExpanders.Remove(typeof<JsonDocument>)
let rec expander key value =
    printfn "%s" "test"
    seq {
        match value with
        | JsonValue.String  (s) -> yield key,typeof<string>,box s
        | JsonValue.Boolean (b) -> yield key,typeof<bool>,box b
        | JsonValue.Float   (f) -> yield key,typeof<float>,box f
        | JsonValue.Null    (_) -> yield key,typeof<obj>,box ()
        | JsonValue.Number  (n) -> yield key,typeof<decimal>,box n
        | JsonValue.Record  (r) ->
            printfn "%A" (value.Properties())
            yield! value.Properties() |> Seq.collect ((<||)expander)
        | JsonValue.Array   (a) ->
            yield! a
            |> Seq.collect (expander "arrayItem")
    }
Frame.CustomExpanders.Add(typeof<JsonDocument>, fun o -> (o :?> JsonDocument).JsonValue |> expander "root")
Frame.CustomExpanders.Add(typeof<JsonValue>, fun o ->  o :?> JsonValue |> expander "root")
(**
Expander code omitted, but can be found [here](https://github.com/fslaborg/FsLab/issues/14)
*)

let dataFrame =
    [for l in congressmen -> series [ "It" => l]]
    |> Frame.ofRowsOrdinal
    |> Frame.expandAllCols 10

(**
---
- data-background : #293c3d url('/images/watermark.svg') 0 0 repeat

#### Deedle: Normal syntax
*)

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

(**

---
- data-background : #293c3d url('/images/watermark.svg') 0 0 repeat

#### Deedle: Quick manipulations

Simple statistics
*)

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

(**
' Note: Don't do this. A linear regression on exports makes 0 sense.

---
- data-background : #293c3d url('/images/watermark.svg') 0 0 repeat

#### Results

Stats 
*)
(*** include-value: stats ***)

(** Regression fit (intercept, slope) *)
(*** include-value: regression ***)

(**
---
- data-background : #293c3d url('/images/watermark.svg') 0 0 repeat

#### R Squared
*)

(*** hide ***)
let regressedValues = highTechFrame.RowKeys |> Seq.map (fun rk -> float rk, fst regression + (snd regression) * float rk)
open MathNet.Numerics

(** R squared value *)
let rsquared = GoodnessOfFit.RSquared(regressedValues |> Seq.map snd,
                                      observations    |> Seq.map snd)
(*** include-value: rsquared ***)



(**
---
- data-background : #293c3d url('/images/watermark.svg') 0 0 repeat

#### Deedle: Combining data and additional feature creation
*)

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

(**
---
- data-background : #293c3d url('/images/watermark.svg') 0 0 repeat

#### Deedle: Straight to R

Arrrr

![arrr](/images/arrr.gif)

---
- data-background : #293c3d url('/images/watermark.svg') 0 0 repeat

#### Deedle: Straight to R
*)

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

(*** include-value: rFrameText ***)

(**
***
- data-background : #664659 url('images/watermark.svg') 0 0 repeat

### Step three: Science

![science](/images/science.jpg)

' So what does the sciencing of data have to do with F#?
' Set precedence: I'm not going to go in-depth
' Short slide OK

---
- data-background : #664659 url('images/watermark.svg') 0 0 repeat

#### "Normal" data analysis languages

![r](/images/r.png)
![python](/images/python.png)

' Historically, deep analysis of data has been done with other languages. R, Python are very popular now.
' Also not on this list are Matlab and SAS

---
- data-background : #664659 url('images/watermark.svg') 0 0 repeat

#### F# is slowly catching up

Machine Learning Algorithms (suite)

![accord](/images/accord.png)

' Cesar Souza updates his Accord .NET ML framework

---
- data-background : #664659 url('images/watermark.svg') 0 0 repeat

#### F# is slowly catching up

Natural Language Processing

![stanford NLP](/images/stanfordnlp.jpg)

' Sergey Tihon converted Stanford NLP library to F#

---
- data-background : #664659 url('images/watermark.svg') 0 0 repeat

#### F# is slowly catching up

Cloud computing

![m-brace](/images/mbrace.png)

' 
*)

(**
***
- data-background : #007b89 url('/images/watermark.svg') 0 0 repeat

### Step four: Visualize
*)

(**
---
- data-background : #007b89 url('/images/watermark.svg') 0 0 repeat

#### Visualizing our previous information

With Google Charts
*)

(*** define-output: pie ***)
let pieChart =
    congressmen
    |> Seq.filter (fun c -> try c.MissedVotes >= 0 with _ -> false)
    |> Seq.groupBy (fun c -> c.Party)
    |> Seq.map (fun g -> fst g, (snd g) |> Seq.sumBy (fun c -> c.MissedVotes))
    |> XPlot.GoogleCharts.Chart.Pie
(**
---
- data-background : #007b89 url('/images/watermark.svg') 0 0 repeat

#### Visualizing our previous information

![pie](/images/pie.png)
*)


(**
---
- data-background : #007b89 url('/images/watermark.svg') 0 0 repeat

#### Visualizing our previous information

*)

(*** define-output: scatter ***)
let scatter =
    exportFrame?``Total Exports``
    |> Series.observations
    |> XPlot.GoogleCharts.Chart.Scatter

(**
---
- data-background : #007b89 url('/images/watermark.svg') 0 0 repeat

#### Visualizing our previous information

![scatter](/images/scatter.png)

*)

(**
***
- data-background : #293c3d url('/images/watermark.svg') 0 0 repeat

### Summary

### Resources

Presentation code: [https://github.com/mandest/FSharpAndDataTalk](https://github.com/mandest/FSharpAndDataTalk)
Presentation share: [http://www.slideshare.net/JonathanDexter/fsharp-and-data-101](http://www.slideshare.net/JonathanDexter/fsharp-and-data-101)

More F# Resources:
* [F# Guides on fsharp.org](http://fsharp.org/)
* [Functional Programming Slack](http://fpchat.com/)
* [F# Weekly](https://sergeytihon.wordpress.com/category/f-weekly/)
*)
