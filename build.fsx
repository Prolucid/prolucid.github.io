// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------
#I @"packages/build/FAKE/tools"
#r @"FakeLib.dll"

open Fake
open Fake.Git
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper
open Fake.UserInputHelper
open System
open System.IO


// Short summary of the project
let description = "Prolucid tech portal"

// Git configuration (used for publishing site in gh-pages branch)
// The profile where the project is posted
let gitOwner = "Prolucid"
let gitHome = sprintf "%s/%s" "https://github.com" gitOwner

// The name of the project on GitHub
let gitName = "prolucid.github.io"

// The url for the raw files hosted
let gitRaw = environVarOrDefault "gitRaw" "https://raw.githubusercontent.com/Prolucid"



Target "Clean" (fun _ ->
    CleanDirs ["temp"; "bin"; "site/output"]
)

// --------------------------------------------------------------------------------------
// Generate the site


let fakePath = "packages" </> "build" </> "FAKE" </> "tools" </> "FAKE.exe"
let fakeStartInfo script workingDirectory args fsiargs environmentVars =
    (fun (info: System.Diagnostics.ProcessStartInfo) ->
        info.FileName <- System.IO.Path.GetFullPath fakePath
        info.Arguments <- sprintf "%s --fsiargs -d:FAKE %s \"%s\"" args fsiargs script
        info.WorkingDirectory <- workingDirectory
        let setVar k v =
            info.EnvironmentVariables.[k] <- v
        for (k, v) in environmentVars do
            setVar k v
        setVar "MSBuild" msBuildExe
        setVar "GIT" Git.CommandHelper.gitPath
        setVar "FSI" fsiPath)

/// Run the given buildscript with FAKE.exe
let executeFAKEWithOutput workingDirectory script fsiargs envArgs =
    let exitCode =
        ExecProcessWithLambdas
            (fakeStartInfo script workingDirectory "" fsiargs envArgs)
            TimeSpan.MaxValue false ignore ignore
    System.Threading.Thread.Sleep 1000
    exitCode

// site
let buildSiteTarget fsiargs target =
    trace (sprintf "Building site (%s), this could take some time, please wait..." target)
    let exit = executeFAKEWithOutput "site/tools" "generate.fsx" fsiargs ["target", target]
    if exit <> 0 then
        failwith "generating site failed"
    ()


let generateSite fail debug =
    let args =
        if debug then "--define:HELP"
        else "--define:RELEASE --define:HELP"
    try
        buildSiteTarget args "Default"
        traceImportant "Site generated"
    with
    | e when not fail ->
        traceImportant "generating site failed"


Target "KeepRunning" (fun _ ->
    use watcher = !! "site/content/**/*.*" |> WatchChanges (fun changes ->
         generateSite true true
    )

    traceImportant "Waiting for site edits. Press any key to stop."

    System.Console.ReadKey() |> ignore

    watcher.Dispose()
)

Target "GenerateSiteDebug" (fun _ ->
    generateSite true true
)

Target "GenerateSite" (fun _ ->
    generateSite true false
)

let createIndexFsx lang =
    let content = """(*** hide ***)
// This block of code is omitted in the generated HTML site. Use
// it to define helpers that you do not want to show in the site.
#I "../../../bin"

(**
=========================
*)
"""
    let targetDir = "site/content" </> lang
    let targetFile = targetDir </> "index.fsx"
    ensureDirectory targetDir
    System.IO.File.WriteAllText(targetFile, System.String.Format(content, lang))

Target "AddLangSite" (fun _ ->
    let args = System.Environment.GetCommandLineArgs()
    if args.Length < 4 then
        failwith "Language not specified."

    args.[3..]
    |> Seq.iter (fun lang ->
        if lang.Length <> 2 && lang.Length <> 3 then
            failwithf "Language must be 2 or 3 characters (ex. 'de', 'fr', 'ja', 'gsw', etc.): %s" lang

        let templateFileName = "template.cshtml"
        let templateDir = "site/tools/templates"
        let langTemplateDir = templateDir </> lang
        let langTemplateFileName = langTemplateDir </> templateFileName

        if System.IO.File.Exists(langTemplateFileName) then
            failwithf "Documents for specified language '%s' have already been added." lang

        ensureDirectory langTemplateDir
        Copy langTemplateDir [ templateDir </> templateFileName ]

        createIndexFsx lang)
)

// --------------------------------------------------------------------------------------
// Release Scripts

Target "ReleaseSite" (fun _ ->
    let tempSiteDir = "temp/gh-pages"
    CleanDir tempSiteDir
    Repository.cloneSingleBranch "" (gitHome + "/" + gitName + ".git") "master" tempSiteDir

    CopyRecursive "site/output" tempSiteDir true |> tracefn "%A"
    StageAll tempSiteDir
    Git.Commit.Commit tempSiteDir ("Update site")
    Branches.push tempSiteDir
)

#load "paket-files/build/fsharp/FAKE/modules/Octokit/Octokit.fsx"
open Octokit

// --------------------------------------------------------------------------------------
// Run all targets by default. Invoke 'build <Target>' to override

Target "All" DoNothing

"GenerateSite"
  ==> "All"
  =?> ("ReleaseSite",isLocalBuild)

"GenerateSiteDebug"
  ==> "KeepRunning"

"Clean"
  ==> "ReleaseSite"

RunTargetOrDefault "All"
