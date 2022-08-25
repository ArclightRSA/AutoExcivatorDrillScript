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
    partial class Program1 : MyGridProgram
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

        bool XAxisJobActive;
        bool ZAxisJobActive;
        bool YAxisJobActive;

        IMyPistonBase piston;

        List<IMyTerminalBlock> blocks;

        List<IMyPistonBase> XPistons;
        List<IMyPistonBase> ZPistons;
        List<IMyPistonBase> YPistons;

        //Piston Variables
        float PVelocity;
        float PMinLimit;
        float PMaxLimit;

        public Program1()
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
            ZAxisJobActive = false;
            YAxisJobActive = false;

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

        public void CheckPistonPos()
        {
            var XPos = XPistons[0].CurrentPosition;
            var XCountPos = XPos * XPistons.Count;

            Echo("---X---");
            Echo(XCountPos.ToString() + " m");
            Echo("---Z---");
            var blocks = XCountPos / 1.5;
            Echo("---Pistons---");
            Echo("X piston Count: " + XPistons.Count);
            Echo("Z piston Count: " + ZPistons.Count);
            Echo("---Blocks---");
            Echo(blocks.ToString() + " Blocks");
        }

        public void PistonSwitch(string coord)
        {
            if (coord == "x")
            {
                foreach (IMyPistonBase p in XPistons)
                {
                }
            }
        }

        public void AxisJobs()
        {
            Echo("---Jobs Pistons---");
            Echo("---X---");
            Echo("Status: " + XPistons[0].Status);
            Echo("X - Piston Count: " + XPistons.Count);
            Echo("X - Current Pos : " + XPistons[0].CurrentPosition);
            if (!XAxisJobActive && !ZAxisJobActive)
            {
                XAxisJob();
            }
            Echo("---Z---");
            Echo("Status: " + ZPistons[0].Status);
            Echo("Z - Piston Count: " + ZPistons.Count);
            Echo("Z - Current Pos : " + ZPistons[0].CurrentPosition);
            if ((XPistons[0].Status == PistonStatus.Extended || XPistons[0].Status == PistonStatus.Retracted))
            {
                ZAxisJob();
            };
        }

        public void XAxisJob()
        {
            var xCurrentPosition = XPistons[0].CurrentPosition;

            if (XAxisJobActive && (XPistons[0].Status == PistonStatus.Extended || XPistons[0].Status == PistonStatus.Retracted))
            {

            }
            else if (!XAxisJobActive && !ZAxisJobActive)
            {
                if ((xCurrentPosition == XPistons[0].MinLimit) && (XPistons[0].Status == PistonStatus.Extended || XPistons[0].Status == PistonStatus.Retracted))
                { //Extend
                    XAxisJobActive = true;
                    ZAxisJobActive = false;
                    foreach (IMyPistonBase p in XPistons)
                    {
                        p.Extend();
                    }
                }
                else if ((xCurrentPosition == XPistons[0].MaxLimit) && (XPistons[0].Status == PistonStatus.Extended || XPistons[0].Status == PistonStatus.Retracted))
                {
                    //Retract
                    XAxisJobActive = true;
                    ZAxisJobActive = false;
                    foreach (IMyPistonBase p in XPistons)
                    {
                        p.Retract();
                    }
                }
            }
        }

        public void ZAxisJob()
        {
            Echo("Z Switch");

            if (ZAxisJobActive && (ZPistons[0].Status == PistonStatus.Extended || ZPistons[0].Status == PistonStatus.Retracted))
            {
                ZAxisJobActive = false;
            }
            else if (!ZAxisJobActive)
            {
                //Extend
                foreach (IMyPistonBase p in ZPistons)
                {
                    XAxisJobActive = false;
                    ZAxisJobActive = true;
                    p.MaxLimit = 2;
                    p.Extend();
                }
            }
        }

        public void YAxisJob()
        {
        }

        //******************************************************************
    }
}
