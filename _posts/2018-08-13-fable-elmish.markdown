---
layout: post
title:  "fable-elmish 2.0 beta is out!"
date:   2018-08-13 10:15:21 -0400
categories: F# fable elmish
---

A beta release of fable-elmish for Fable2 has been published over the weekend and is [available on nuget](https://www.nuget.org/packages?q=fable.elmish). This release deprecates earlier changes in certain API's order of arguments and should be mostly a straight-forward upgrade for those targeting Fable2. Currently there are minor breaking changes in the full signatures of Debugger API only, so the upgrade should have minimal impact on the existing code-base. 

Please also note that there are potential issues related to the changes in the way Fable2 handles type information and how instance methods are attached, please report via github if you hit them.