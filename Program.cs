﻿using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
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
    partial class Program : MyGridProgram
    {
        //******************************************************************
        // -- Main
        string main_tag = "Ex/";
        // Piston Pistions
        string XTag = "X/";
        string ZTag = "Z/";
        string YTag = "Y/";

        bool programSet;
        bool programState;
        bool LayerDone;

        bool XAxisJobActive;
        bool ZAxisJobActive;
        bool YAxisJobActive;

        bool XAxisJobDone;
        bool ZAxisJobDone;

        IMyPistonBase piston;

        List<IMyTerminalBlock> blocks;

        List<IMyPistonBase> XPistons;
        List<IMyPistonBase> ZPistons;
        List<IMyPistonBase> YPistons;

        //Piston Variables
        float PVelocity;
        float PMinLimit;
        float PMaxLimit;

        public Program()
        {
            Echo("Program");

            programSet = false;
            programState = false;

            // Enable Runtime
            //Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        public void Main(string argument, UpdateType updateSource)
        {

            if (argument == "Set")
            {
                SetSystem();
            }
            else if (argument == "Reset" && programState)
            {
                Echo("Reset");
            }
            else if (argument == "Stop" && programState)
            {
                programSet = false;
                programState = false;
                Runtime.UpdateFrequency = UpdateFrequency.None;
            }
            else if (argument == "Start" && programSet)
            {
                Echo("----------");
                Echo("Starting");
                Echo("--------------");
                programState = true;
            }
            else if (argument == "X" && programState)
            {
                XAxisJobDone = false;
                XAxisJobActive = false;
            }
            else if (argument == "Z" && programState)
            {
                ZAxisJobActive = false;
            }
            else if (programSet && programState)
            {
                Echo("----------");
                Echo("Running");
                Echo("--------------");
                AxisJobs();
            }
            else if (programSet)
            {
                Echo("----------");
                Echo("Setup Done - Ready to Start!");
                Echo("--------------");
            }
            else
                Echo("Setup Program First");
        }

        public void SetSystem()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            programState = false;

            Echo("----------");
            Echo("SetSystem");
            Echo("----------");

            //Piston Setup
            //PVelocity = -0.1F;

            XAxisJobActive = false;
            XAxisJobDone = false;
            ZAxisJobActive = false;
            ZAxisJobDone = false;
            YAxisJobActive = false;

            LayerDone = false;

            PVelocity = -1.0F;
            PMinLimit = 0;
            PMaxLimit = 10;

            XPistons = new List<IMyPistonBase>();
            ZPistons = new List<IMyPistonBase>();
            YPistons = new List<IMyPistonBase>();

            blocks = new List<IMyTerminalBlock>();

            GridTerminalSystem.SearchBlocksOfName(main_tag, blocks);

            try
            {
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
                            piston.Enabled = true;
                            XPistons.Add(piston);
                        }
                        else if (piston.CustomName.Contains(ZTag))
                        {
                            if (!piston.GetValueBool("ShareInertiaTensor"))
                            {
                                piston.GetActionWithName("ShareInertiaTensor").Apply(piston);
                            }
                            piston.Velocity = PVelocity;
                            piston.MinLimit = PMinLimit;
                            piston.MaxLimit = PMinLimit;
                            piston.Enabled = true;
                            ZPistons.Add(piston);
                        }
                        else if (piston.CustomName.Contains(YTag))
                        {
                            if (!piston.GetValueBool("ShareInertiaTensor"))
                            {
                                piston.GetActionWithName("ShareInertiaTensor").Apply(piston);
                            }
                            piston.Velocity = PVelocity;
                            piston.MinLimit = PMinLimit;
                            piston.MaxLimit = PMaxLimit;
                            piston.Enabled = true;
                            YPistons.Add(piston);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Echo(ex.Message);
            }
            programSet = true;
        }

        public void AxisJobs()
        {
            Echo("---X---");
            Echo("Status: " + XPistons[0].Status);
            Echo("X - Current Pos : " + XPistons[0].CurrentPosition);
            Echo("---Z---");
            Echo("Z - Current Pos : " + ZPistons[0].CurrentPosition);
            if (!LayerDone)
            {
                XAxisJob();
            }                    
            
                  
        }

        public void XAxisJob()
        {
            if (XAxisJobActive && (XPistons[0].Status == PistonStatus.Retracted || XPistons[0].Status == PistonStatus.Extended))
            {
                ZAxisJob();
            }
            else if (!XAxisJobActive)
            {
                XAxisJobActive = true;

                if (XPistons[0].Status == PistonStatus.Retracted)
                {
                    //XAxisJobActive = true;
                    //Extend
                    foreach (IMyPistonBase p in XPistons)
                    {
                        var s = p.Status;
                        
                        p.Extend();
                    }
                }
                else if (XPistons[0].Status == PistonStatus.Extended)
                {
                    //Retract
                    foreach (IMyPistonBase p in XPistons)
                    {
                        var s = p.Status;
                        p.Retract();
                    }
                }
            }
        }

        public void ZAxisJob()
        {
            
            if (ZPistons[0].MaxLimit == 10 && (ZPistons[0].Status == PistonStatus.Retracted || ZPistons[0].Status == PistonStatus.Extended))
            {
                LayerDone = true;
                ZAxisJobDone = false;
                ZAxisJobActive = false;
                XAxisJobActive = false;

                ResetAxisXZ();
            }
            else if (ZAxisJobActive && (ZPistons[0].Status == PistonStatus.Retracted || ZPistons[0].Status == PistonStatus.Extended))
            {
                ZAxisJobActive = false;
                XAxisJobActive = false;
            }
            else if (!ZAxisJobActive)
            {
                foreach (IMyPistonBase p in ZPistons)
                {
                    ZAxisJobActive = true;
                    p.MaxLimit += 2;
                    p.Extend();
                }
            }
        }

        public void ResetAxisXZ()
        {
            ZAxisJobActive = true;
            XAxisJobActive = true;

            foreach (IMyPistonBase p in XPistons)
            {
                var s = p.Status;

                p.Retract();
            }
            foreach (IMyPistonBase p in ZPistons)
            {
                p.MaxLimit = 0;
                p.Retract();
            }
        }
        //******************************************************************
    }
}
