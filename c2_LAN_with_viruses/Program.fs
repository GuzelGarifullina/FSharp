﻿(* LAN - third week homework
   Author Guzel Garifullina 171
   Estimated time 2 hour
   real time      5 hours
 *)

open System
type IComputer = 
    interface
        abstract Os      : string
        abstract Defence :  int
        abstract Infected : bool with get, set
    end
type Computer (os : string) =
    class
        let os_to_defence (os : string) : int =
            match os with
            | "Windows" -> 38
            | "Linux"   -> 72
            | "Solaris" -> 66
            | "Android" -> 68
            | _         -> 10
         
        interface IComputer with
            member val Os = os 
            member val Defence = os_to_defence os 
            member val Infected = false with get,set
    end

type ILAN = 
    interface
        /// number of computers
        abstract Init         : unit -> unit
        abstract Number       : int
        /// shows is in LAN unaffected computers
        abstract Alive        : bool with get, set
        ///only close neighbours
        abstract IsWay        : int  -> int -> bool
        abstract HealthyNeigh : int  -> int list
        abstract VirusAttack  : unit -> unit
        abstract PrintState   : unit -> unit
    end 

type LAN (net : bool [,] , os_arr: string array) =
    class
        let n = Array2D.length1 net
        let computers = Array.init n (fun x -> new Computer ( os_arr.[x]):> IComputer )

        let victims_array = Array.create  n false

        static let rand = new System.Random()
        ///list of computers that will be attackted in the next step
        let mutable victims_list  = []
   
        interface ILAN with
            member this.Init () =
                let id_infected = (rand.Next () ) % (n - 1)
                do computers.[id_infected].Infected <- true
                victims_list  <- (this :> ILAN).HealthyNeigh id_infected
            member this.Number = n
            member val Alive = true with get, set
            member this.IsWay i j = net.[i,j]
            member this.HealthyNeigh i =
                let mutable neigh = []
                for j = (n - 1) downto 0 do
                    if  ((this :> ILAN).IsWay i j ) && (not computers.[j].Infected)
                                && (not victims_array.[j])
                    then neigh <- j :: neigh
                         victims_array.[j] <- true     
                    else ()
                neigh
            member this.VirusAttack () = 
               let rec virus_attack (victims_list : int list)  = 
                   let attack () = (rand.Next () ) % 101
                   match victims_list with
                   | [] -> [] 
                   | i :: victims_list -> 
                       let attack1 = attack ()  
                       if (attack1  > computers.[i].Defence)
                        then computers.[i].Infected <- true 
                             ((this :> ILAN).HealthyNeigh i) @ (virus_attack victims_list) 
                        else i :: (virus_attack victims_list)
               victims_list <- virus_attack victims_list 
               if (victims_list = [])
                   then (this :> ILAN).Alive <- false
            member this.PrintState ()=
               let make_os_name i =
                   match (computers.[i]).Infected with
                   | true -> Char.ToString (os_arr.[i].[0])  + "!"
                   | _    -> Char.ToString (os_arr.[i].[0])  + " " 

               let os_names = Array.init  n make_os_name 
               let format = """
                {0}------{1}      {2}------ {3}
                |       |       |        |
                |       |       |        |
                {4}------{5}------{6}       {7}
                        / \    /           
                       /   \  /    
                       {8}---{9}"""  
               let names = Array.map (fun x -> x:>obj) os_names
               do printfn "System state"
               System.Console.WriteLine (String.Format(format, names))
        
    end
[<EntryPoint>]
let main argv = 
    let array = Array2D.create 10 10 false 
    let ex x y = Array2D.set array x y true
                 Array2D.set array y x true
    ex 0 1
    ex 0 4
    ex 1 5
    ex 2 3
    ex 2 6
    ex 3 7
    ex 4 5
    ex 5 6
    ex 5 8
    ex 5 9
    ex 6 9
     
    let list = [| "Android"; "Windows"; "Solaris"; "Other";
                 "Solaris"; "Linux"  ; "Linux"  ; "Linux";
                 "Windows"; "Android"|]
    let net = new LAN (array, list) :>ILAN
    net.Init ()
    net.PrintState ()
    while net.Alive do
        net.VirusAttack ()
        net.PrintState ()
            
    0        