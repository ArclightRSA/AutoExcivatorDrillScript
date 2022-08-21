using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class ProgramRef : MyGridProgram
    {
        //******************************************************************

        //Tag Setup
        // -- Main
        string main_tag = "Ex/";
        // Piston Pistions
        string XTag = "X/";

        //Piston Variables
        float PVelocity;
        float PMinLimit;
        float PMaxLimit;

        bool PistonXJob;

        bool PistonXExtend;

        bool canStart = false;
        bool programRunning = false;

        IMyPistonBase piston;

        //Piston Setup
        List<IMyTerminalBlock> blocks;

        int XPistonMax = 0;
        List<IMyPistonBase> XPistons;

        public ProgramRef()
        {
            Echo("Program");
            // Enable Runtime
            //Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            Echo("Main");

            if (argument == "Start" && canStart)
            {
                Echo("Start!");
                if (PistonXJob == false)
                {
                    InitialStartSystem();
                }

            }
            else if (argument == "Set")
            {
                Echo("Setting Up!");
                SetSystem();
            }
            else if (argument == "Stop")
            {
                programRunning = false;
                Runtime.UpdateFrequency = UpdateFrequency.None;
            }
            else if (argument == "PrintX" && canStart)
            {
                int count = 0;
                foreach (IMyPistonBase pis in XPistons)
                {
                    count++;
                }
                try
                {
                    Echo("Piston Count: " + count);
                }
                catch (Exception ex)
                {

                    Echo("Print Error: " + ex);
                }

            }
            if (programRunning)
            {
                CheckPistonPistions();
            }
        }

        public void SetSystem()
        {
            //Setup Bools
            PistonXJob = false;
            PistonXExtend = false;

            //Piston Setup
            //PVelocity = -0.1F;
            PVelocity = -5.0F;
            PMinLimit = 0;
            PMaxLimit = 10;

            Echo("Busy Setting Up...");
            XPistonMax = 0;
            XPistons = new List<IMyPistonBase>();
            blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.SearchBlocksOfName(main_tag, blocks);

            foreach (IMyTerminalBlock b in blocks)
            {
                if (b is IMyPistonBase)
                {
                    piston = b as IMyPistonBase;

                    if (piston.CustomName.Contains(XTag))
                    {
                        if (!piston.GetValueBool("ShareInertiaTensor"))
                        {
                            piston.GetActionWithName("ShareInertiaTensor").Apply(piston);
                        }
                        piston.Velocity = PVelocity;
                        piston.MinLimit = PMinLimit;
                        piston.MaxLimit = PMaxLimit;
                        XPistons.Add(piston);
                    }
                }
            }

            XPistonMax = XPistons.Count * 6;
            Echo("Done Setting Up!");
            Echo("Piston Count: " + XPistons.Count);
            Echo("X - Max = " + XPistonMax + " m");
            canStart = true;
        }

        public void CheckPistonPistions()
        {
            var XPos = XPistons[0].CurrentPosition;
            var XCountPos = XPos * XPistons.Count;

            Echo("---X---");
            Echo(XCountPos.ToString() + " m");
            Echo("---Z---");
            /*
                var blocks = XCountPos / 2.5;
                Echo("---Blocks---");
                Echo(blocks.ToString() + " Blocks");
            */
        }

        public void InitialStartSystem()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            PistonXJob = true;
            PistonSwitch("x");
        }

        public void PistonGoals()
        {
            var xPos = XPistons[0].CurrentPosition;

            if (PistonXJob)
            {
                if (xPos == 10 && PistonXExtend)
                {
                    PistonSwitch("x");
                }
                else if (xPos == 0 && !PistonXExtend)
                {
                    PistonSwitch("x");
                }
            }
        }

        public void PistonSwitch(string coord)
        {
            if(coord == "x")
            {
                foreach (IMyPistonBase p in XPistons)
                {
                    if(PistonXExtend)
                    {
                        p.Extend();
                        PistonXExtend = true;
                    }
                    else
                    {
                        p.Retract();
                        PistonXExtend = false;
                    }
                }
            }
        }

        //******************************************************************
    }
}
