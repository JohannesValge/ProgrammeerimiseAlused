# RSS-voogude agregaator

Programmeerimise aluste kodutöö 2. C# konsooliprogramm loeb nelja väljaande RSS-vooge, sorteerib uudised LINQ-iga avaldamisaja järgi ning salvestab 20 uusimat uudist faili `uudised.html`. Iga loendi element sisaldab lingitud pealkirja, avaldamisaega koos ajavööndiga, väljaande nime ja kirjeldust.

## Käivitamine

Vajalik on .NET 10 SDK ja internetiühendus. Ava terminal selles kaustas:

```powershell
dotnet run
```

Fail salvestatakse terminali aktiivsesse kausta ja avatakse vaikimisi brauseris. Ilma brauserit avamata:

```powershell
dotnet run -- --no-browser
```

Kui üks voog ebaõnnestub, jätkab programm teistega. Kui uudiseid on vähem kui 20, salvestatakse kõik leitud uudised. Puuduva või vigase kuupäeva ja lingiga uudised jäetakse vahele. Kui uudiseid ei leita, lõpeb programm veakoodiga 1 ja olemasolevat HTML-faili ei kirjutata üle.

`RssItem` kirjeldab üht uudist. `HttpClient` laadib XML-i, `XDocument` loeb uudise väljad, `List<RssItem>` hoiab uudiseid ning `OrderByDescending(...).Take(20)` valib uusimad. `FileStream` ja `StreamWriter` kirjutavad HTML-i `foreach`-tsüklis. `using` vabastab ressursid. HTML-i lisatavad tekstid kodeeritakse, et uudisvoo sisu ei muutuks käivitatavaks HTML-iks.

## GitHub

Kopeeri see kaust oma Programmeerimise aluste GitHubi repo kohalikku koopiasse, lisa failid commit'i ja saada muudatused GitHubi. `bin`, `obj` ja genereeritud uudiste fail on `.gitignore`-is.
