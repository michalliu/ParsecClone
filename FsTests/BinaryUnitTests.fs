﻿module BinaryUnitTests

open NUnit.Framework
open FsUnit
open System.IO
open Combinator
open BinaryCombinator

[<Test>]
let binTest1() = 
    let bytes = [|0;1;2;3;4;5;6;7;8|] |> Array.map byte

    let stream = new MemoryStream(bytes)   

    let parserStream = new BinStream(stream)

    let parser = byte1

    let result = test parserStream parser 
    
    result |> should equal 0

let byteListToArr arr = 
    arr |> Array.toSeq |> Seq.take 4 |> Seq.toArray |> Array.map byte

[<Test>]
let binTest2() = 
    let bytes = [|0;1;2;3;4;5;6;7;8|] |> Array.map byte

    let stream = new MemoryStream(bytes)   

    let parserStream = new BinStream(stream)

    let parser = manyN 4 byte1 

    let result = test parserStream parser 
    
    result |> should equal (bytes |> byteListToArr)

[<Test>]
let binTest3() = 
    let bytes = [|0;1;2;3;4;5;6;7;8|] |> Array.map byte

    let stream = new MemoryStream(bytes)   

    let parserStream = new BinStream(stream) 

    let parser = manyN 2 byte4

    let result = test parserStream parser 
    
    result |> should equal [[|0;1;2;3|];[|4;5;6;7|]]

[<Test>]
let binTest4() = 
    let bytes = [|0;1;2;3;4;5;6;7;8|] |> Array.map byte

    let stream = new MemoryStream(bytes)   

    let parserStream = new BinStream(stream)   

    let result = test parserStream int32 
    
    result |> should equal 50462976


[<Test>]
let binaryWithBacktracker() = 
    let bytes = [|0;1;2;3;4;5;6;7;8|] |> Array.map byte

    let stream = new MemoryStream(bytes)   

    let parserStream = new BinStream(stream)   

    let failureParse =  manyN 10 int32

    let backtrackWithFail = int32 >>=? fun b1 -> 
                            failureParse >>= fun b2 -> 
                            preturn b1

    let consume1 = backtrackWithFail .>>. byte1

    let result = test parserStream consume1
    
    result |> should equal (50462976, byte(4))

[<Test>]
let ``test backtracker with attempt operator``() = 
    let bytes = [|0;1;2;3;4;5;6;7;8|] |> Array.map byte

    let stream = new MemoryStream(bytes)   

    let parserStream = new BinStream(stream)   

    let shouldFail = int32 >>= fun b1 -> 
                               manyN 10 int32 >>= fun b2 -> 
                               preturn b1

    let consume1 = choice[attempt shouldFail; intB]

    let result = test parserStream consume1
    
    result |> should equal 0

[<Test>]
let takeTillTest() = 
    let bytes = [|0;1;2;3;4;5;6;7;8|] |> Array.map byte

    let stream = new MemoryStream(bytes)   

    let parserStream = new BinStream(stream)   

    let takenLower = takeTill (fun i -> i >= byte(4)) byte1
    
    let result = test parserStream takenLower
    
    result |> should equal ([|0;1;2;3|] |> Array.map byte)

[<Test>]
let takeTillTest2() = 
    let bytes = [|0;1;2;3;4;5;6;7;8|] |> Array.map byte

    let stream = new MemoryStream(bytes)   

    let parserStream = new BinStream(stream)   

    let takeUpper = takeTill (fun i -> i >= byte(4)) byte1 >>. byteN 5    

    let result = test parserStream takeUpper
    
    result |> should equal ([|4;5;6;7;8|] |> Array.map byte)


[<Test>]
let takeWhileTest() = 
    let bytes = [|0;1;2;3;4;5;6;7;8|] |> Array.map byte

    let parserStream = new BinStream(new MemoryStream(bytes))   

    let takeUpper = takeWhile (fun i -> i < byte(4)) byte1 >>. byteN 5    

    let result = test parserStream takeUpper
    
    result |> should equal ([|4;5;6;7;8|] |> Array.map byte)

[<Test>]
let endianessTest() = 
    let bytes = [|0x11;0x7B;0;0;0;0;0;0|] |> Array.map byte

    let parserStream = new BinStream(new MemoryStream(bytes), Array.rev)   

    let result = test parserStream uint16
    
    result |> should equal 4475


[<Test>]
let endianessTest2() = 
    let bytes = [|0xF1;0x7B;0;0;0;0;0;0|] |> Array.map byte

    let parserStream = new BinStream(new MemoryStream(bytes), Array.rev)   

    let result = test parserStream int16
    
    result |> should equal -3717
