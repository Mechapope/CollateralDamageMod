using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MeowDSIO;
using MeowDSIO.DataFiles;
using MeowDSIO.DataTypes;
using System.Reflection;

namespace CollateralDamageMod
{
    class Program
    {
        static void Main(string[] args)
        {
            List<PARAMDEF> ParamDefs = new List<PARAMDEF>();
            List<PARAM> AllParams = new List<PARAM>();
            //var gameFolder = @"D:\Program Files (x86)\Steam\steamapps\common\Dark Souls Prepare to Die Edition\DATA\";
            var gameFolder = @"C:\Users\mcouture\Desktop\DS-Modding\Dark Souls Prepare to Die Edition\DATA\";

            var gameparamBnds = Directory.GetFiles(gameFolder + "param\\GameParam\\", "*.parambnd")
                .Select(p => DataFile.LoadFromFile<BND>(p, new Progress<(int, int) > ((pr) =>
                {

                })));

            List<BND> PARAMBNDs = gameparamBnds.ToList();

            List<BND> allMsgBnds = Directory.GetFiles(gameFolder + "msg\\ENGLISH\\", "*.msgbnd")
                .Select(p => DataFile.LoadFromFile<BND>(p, new Progress<(int, int) > ((pr) =>
                {

                }))).ToList();

            var paramdefBnds = Directory.GetFiles(gameFolder + "paramdef\\", "*.paramdefbnd")
                .Select(p => DataFile.LoadFromFile<BND>(p, new Progress<(int, int) > ((pr) =>
                {

                }))).ToList();

            for (int i = 0; i < paramdefBnds.Count(); i++)
            {
                foreach (var paramdef in paramdefBnds[i])
                {
                    PARAMDEF newParamDef = paramdef.ReadDataAs<PARAMDEF>(new Progress<(int, int) > ((r) =>
                    {

                    }));
                    ParamDefs.Add(newParamDef);
                }
            }

            for (int i = 0; i < PARAMBNDs.Count(); i++)
            {
                foreach (var param in PARAMBNDs[i])
                {
                    PARAM newParam = param.ReadDataAs<PARAM>(new Progress<(int, int) > ((p) =>
                    {

                    }));

                    newParam.ApplyPARAMDEFTemplate(ParamDefs.Where(x => x.ID == newParam.ID).First());

                    AllParams.Add(newParam);
                }
            }

            foreach (PARAM paramFile in AllParams)
            {
                if (paramFile.ID == "NPC_PARAM_ST")
                {
                    foreach (MeowDSIO.DataTypes.PARAM.ParamRow paramRow in paramFile.Entries)
                    {
                        foreach (MeowDSIO.DataTypes.PARAM.ParamCellValueRef cell in paramRow.Cells)
                        {
                            if (cell.Def.Name == "moveType")
                            {
                                Type type = cell.GetType();
                                PropertyInfo prop = type.GetProperty("Value");
                                prop.SetValue(cell, 6, null);
                            }
                        }
                    }
                }
                else if (paramFile.VirtualUri.EndsWith("AtkParam_Pc.param"))
                {
                    foreach (MeowDSIO.DataTypes.PARAM.ParamRow paramRow in paramFile.Entries)
                    {
                        foreach (MeowDSIO.DataTypes.PARAM.ParamCellValueRef cell in paramRow.Cells)
                        {
                            if (cell.Def.Name == "knockbackDist")
                            {
                                Type type = cell.GetType();
                                PropertyInfo prop = type.GetProperty("Value");
                                prop.SetValue(cell, 5, null);
                            }
                            string[] attackAttrs = { "atkPhys", "atkMag", "atkFire", "atkThun", "atkPhysCorrection", "atkMagCorrection", "atkFireCorrection", "atkThunCorrection" };

                            if (attackAttrs.Contains(cell.Def.Name))
                            {
                                Type type = cell.GetType();
                                PropertyInfo prop = type.GetProperty("Value");
                                prop.SetValue(cell, 0, null);
                            }

                        }
                    }
                }
                else if (paramFile.ID == "NPC_THINK_PARAM_ST")
                {
                    foreach (MeowDSIO.DataTypes.PARAM.ParamRow paramRow in paramFile.Entries)
                    {
                        foreach (MeowDSIO.DataTypes.PARAM.ParamCellValueRef cell in paramRow.Cells)
                        {
                            string[] attrsToMax = { "outDist", "maxBackhomeDist", "backhomeDist", "backhomeBattleDist", "BackHome_LookTargetTime", "BackHome_LookTargetDist", "SightTargetForgetTime", "SoundTargetForgetTime" };
                            if (attrsToMax.Contains(cell.Def.Name))
                            {
                                Type type = cell.GetType();
                                PropertyInfo prop = type.GetProperty("Value");
                                prop.SetValue(cell, cell.Def.Max, null);
                            }
                            else if (cell.Def.Name == "TeamAttackEffectivity")
                            {
                                Type type = cell.GetType();
                                PropertyInfo prop = type.GetProperty("Value");
                                prop.SetValue(cell, cell.Def.Max, null);
                            }


                        }
                    }
                }
                else if (paramFile.ID == "BULLET_PARAM_ST")
                {
                    foreach (MeowDSIO.DataTypes.PARAM.ParamRow paramRow in paramFile.Entries)
                    {
                        foreach (MeowDSIO.DataTypes.PARAM.ParamCellValueRef cell in paramRow.Cells)
                        {
                            if (cell.Def.Name == "isHitBothTeam:1")
                            {
                                Type type = cell.GetType();
                                PropertyInfo prop = type.GetProperty("Value");
                                prop.SetValue(cell, true, null);
                            }
                            //else if (cell.Def.Name == "isPenetrate")
                            //{
                            //    Type type = cell.GetType();
                            //    PropertyInfo prop = type.GetProperty("Value");
                            //    prop.SetValue(cell, 1, null);
                            //}

                        }
                    }
                }
            }
            
            //Resave BNDs
            foreach (var paramBnd in PARAMBNDs)
            {
                foreach (var param in paramBnd)
                {
                    var filteredParamName = param.Name.Substring(param.Name.LastIndexOf("\\") + 1).Replace(".param", "");

                    var matchingParam = AllParams.Where(x => x.VirtualUri == param.Name).First();

                    param.ReplaceData(matchingParam,
                        new Progress<(int, int) > ((p) =>
                        {

                        }));
                }

                DataFile.Resave(paramBnd, new Progress<(int, int) > ((p) =>
                {

                }));
            }

        }
    }
}
