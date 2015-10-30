#load @"packages\FSharp.Charting.0.90.12\FSharp.Charting.fsx"
#r @"packages\MathNet.Numerics.3.8.0\lib\net40\MathNet.Numerics.dll"

// 
//      Creates a chart with three plots -
// 
//       1) A random stock price walk, from a starting point for a fixed number of steps. 
//       2) A simple linear regression plot showing the overall trend of the random walk.
//       3) An oscillator that shows the slope of the trend over a rolling lookback window.
//

open System
open FSharp.Charting
open MathNet.Numerics
open System.Collections
    
//** Parameters **//
let startingPoint = 200             // Stock price random walk starts here
let iterations = 600                // Number of steps in the random walk
let window = 14                     // The lookback window for the oscillator
let oscillatorMagnification = 25.0  // The slope is a number between -1 and 1, magnify it so it's more visible.

// Random number generator, for our random stock price walk.
let rnd = new System.Random()

// Unfolds a random walk 
let walker = 
    startingPoint
    |> Seq.unfold (fun currentValue -> Some (currentValue, Math.Max(1, currentValue + rnd.Next(-10, 12)))) // Slight uptrend
    |> Seq.take(iterations) 
    |> Seq.mapi (fun x y -> Convert.ToDouble(x) , Convert.ToDouble(y)) 

// Capture a fixed random walk 
let randomWalk = Seq.toList(walker)

// LinearRegression to get the intercept and slope
let intercept, slope = MathNet.Numerics.LinearRegression.SimpleRegression.Fit randomWalk

// Unfold a regressionLine
let regressionLine =  
    intercept
    |> Seq.unfold (fun currentValue -> Some (currentValue, currentValue + slope))
    |> Seq.take(iterations) 
    |> Seq.mapi (fun x y -> Convert.ToDouble(x) , Convert.ToDouble(y)) 

// Slope oscillator
let slopeTrend = 
    randomWalk
    |> Seq.windowed(window)
    |> Seq.mapi (fun i x -> i + (window - 1), (snd (MathNet.Numerics.LinearRegression.SimpleRegression.Fit x)) * oscillatorMagnification)

// Plot the chart
Chart.Combine([ Chart.Line randomWalk; Chart.Line regressionLine; Chart.Line slopeTrend])