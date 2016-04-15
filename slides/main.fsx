(** - data-background : #007b89 url('/images/watermark.svg') 0 0 repeat *)

(*** hide ***)
#load "..\packages\FsLab\Fslab.fsx"
open Deedle
let missedVotesUrl = "http://api.nytimes.com/svc/politics/v3/us/legislative/congress/113/house/votes/missed.json"
let apikey = "24f6ff5db83d141f6cb29201033f0cea:8:72625105"

(**
# F# and Data 101
### Utilizing FsLab to Quickly Understand your Data
#### Jonathan Dexter, Technology Manager of .NET, The Nerdery
![nerdery logo](/images/nerdery_logo.png)

' We're going to talk about F# and data.
' Who I am: Talk about being a developer, my current position, and The Nerdery

***
- data-background : #007b89 url('/images/watermark.svg') 0 0 repeat

### Agenda
* What is this talk about
* F# (optional)
* Get the data!
* Engineer!
* Science!
* ???
* Profit!
*)

(**

***
- data-background : #007b89 url('/images/watermark.svg') 0 0 repeat

### If you came here to hear about
* "Monads"
* "Functors"
* Tail-call optimization
* Immutable design
* Pattern matching

***
- data-background : #007b89 url('/images/watermark.svg') 0 0 repeat

![nope](/images/nope_2.gif)

' Time check: 5 min
' Partially kidding: Part of those items are covered in this talk.
' We will not however cover them in-depth.
' We'll touch on some of these, but that's not the focus.

***
- data-background : #007b89 url('/images/watermark.svg') 0 0 repeat

### What we WILL talk about : FsLab

![fslab](/images/fslab.png)

' FsLab isn't one library. It's a set of tools.
' Let's break down those tools...

***
- data-background : #007b89 url('/images/watermark.svg') 0 0 repeat

With Paket
```powershell
paket init
paket add nuget fslab
```

---
- data-background : #007b89 url('/images/watermark.svg') 0 0 repeat

With NuGet

```powershell
nuget install fslab -OutputDirectory packages
```

' Or, of course, you could use Visual Studio to install the package.

***
- data-background : #007b89 url('/images/watermark.svg') 0 0 repeat

### The process
(Not limited to FsLab)

```f#
acquire data
|> engineer
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

| Library | Aqcuire | Engineer | Science | Display |
|---|---|---|---|---|
| F# Data | **Yep!** | Nope | Nope | Nope |
| Deedle | Nope | **Yep** | Nope | Nope |
| .NET Numerics | Nope | *Supports* | *Supports* | Nope |
| R Type Provider | *Partial* | Focus | Focus | *Partial* |
| XPlot | Nope | Nope | Nope | **Yep** |

***
- data-background : #664659 url('images/watermark.svg') 0 0 repeat

### Step one: Aqcuire

---
- data-background : #664659 url('images/watermark.svg') 0 0 repeat

#### Classic scenario: CSV
*)

let data = Deedle.Frame.ReadCsv("path.csv")

(**
' We're going to get this one out of the way because it is so basic.

---
- data-background : #664659 url('images/watermark.svg') 0 0 repeat

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
- data-background : #664659 url('images/watermark.svg') 0 0 repeat

Type Provider Scenario: JSON provider
*)
type JsonContext = FSharp.Data.JsonProvider< """{
  "status": "OK",
  "copyright": "Copyright (c) 2016 The New York Times Company",
  "results": [{
      "congress": "113",
      "chamber": "House",
      "num_results": "450",
      "offset": "0",
      "members": [{
          "id": "M000309",
          "name": "Carolyn McCarthy",
          "party": "D",
          "state": "NY",
          "district": "4",
          "total_votes": "1192",
          "missed_votes": "687",
          "missed_votes_pct": "57.63",
          "rank": "1",
          "notes": "Will retire at the end of 113th Congress."
        }]}]}""">
(**
---
- data-background : #664659 url('images/watermark.svg') 0 0 repeat

Type Provider Scenario: JSON Provider (cont.)
*)

let missedVotes =
  JsonContext.AsyncLoad(sprintf "%s?api-key=%s" missedVotesUrl apikey)
  |> Async.RunSynchronously

missedVotes.Results
|> Seq.collect (fun r -> r.Members)
|> Seq.sortBy (fun m -> try m.MissedVotesPct with | ex -> 0.0m)
|> Seq.rev
|> Seq.skip 1
|> Seq.head

(**

***
### Step one: Engineer
*)

(**
### Step one: Science
*)

(**
### Step one: Visualize
*)
