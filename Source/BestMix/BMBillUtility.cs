﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace BestMix
{
    public class BMBillUtility
    {
        public static string UseBMixMode(CompBestMix compBM, Thing billGiver, Bill bill)
        {
            var mode = "DIS";
            if (compBM != null)
            {
                mode = compBM.CurMode; // defaults to bench Gizmo
                if (compBM?.BillBMModes != null)
                {
                    List<string> BillModeListing = compBM?.BillBMModes;
                    if (BillModeListing.Count > 0)
                    {
                        foreach (var BillMode in BillModeListing)
                        {
                            if (BillValuePart(BillMode) == bill.GetUniqueLoadID())
                            {
                                mode = ModeValuePart(BillMode);
                                if (mode == "NON") { mode = compBM.CurMode; }
                                break;
                            }
                        }
                    }
                }
            }
            return mode;
        }

        public static Texture2D GetBillBMTex(Thing billGiver, Bill bill)
        {
            var mode = GetBillBMMode(billGiver, bill);
            var TexPath = "UI/BestMix/NONIcon";

            if (mode != "NON")
            {
                TexPath = BestMixUtility.GetBMixIconPath(mode);
            }
            TexPath += "Bill";

            //Log.Message("TexPath: " + TexPath);

            Texture2D tex = ContentFinder<Texture2D>.Get(TexPath, false);
            return tex;
        }

        public static string GetBillBMMode(Thing billGiver, Bill bill)
        {
            var mode = "NON";
            if (billGiver != null && (!(billGiver is Pawn p)))
            {
                CompBestMix CBM = billGiver.TryGetComp<CompBestMix>();
                if (CBM != null)
                {
                    var billID = bill?.GetUniqueLoadID();
                    if (billID != null)
                    {
                        if (CBM?.BillBMModes != null)
                        {
                            List<string> BillModes = CBM.BillBMModes;
                            if (BillModes.Count > 0)
                            {
                                foreach (var BillMode in BillModes)
                                {
                                    if (BillValuePart(BillMode) == billID)
                                    {
                                        mode = ModeValuePart(BillMode);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return mode;
        }

        public static void SetBillBMVal(Thing billGiver, Bill bill)
        {
            if (billGiver != null)
            {
                CompBestMix CBM = billGiver.TryGetComp<CompBestMix>();
                if (CBM != null)
                {
                    DoBillModeSelMenu(CBM, bill);
                }
            }
        }

        public static void DoBillModeSelMenu(CompBestMix CBM, Bill bill)
        {
            var list = new List<FloatMenuOption>();

            string text = "BestMix.DoNothing".Translate();
            list.Add(new FloatMenuOption(text, delegate
            {
                SetBMixBillMode(CBM, bill, "NON", true);
            },
            MenuOptionPriority.Default, null, null, 29f, null));

            foreach (var mode in BestMixUtility.BMixModes())
            {
                text = BestMixUtility.GetBMixModeDisplay(mode);
                list.Add(new FloatMenuOption(text, delegate
                {
                    SetBMixBillMode(CBM, bill, mode, true);
                },
                MenuOptionPriority.Default, null, null, 29f, null));
            }
            var sortedlist = list.OrderBy(bm => bm.Label).ToList();
            Find.WindowStack.Add(new FloatMenu(sortedlist));
        }

        public static void SetBMixBillMode(CompBestMix CBM, Bill bill, string mode, bool set)
        {
            if ((CBM != null) && (bill != null))
            {
                var billID = bill.GetUniqueLoadID();
                var newlist = new List<string>();
                if (CBM?.BillBMModes != null)
                {
                    List<string> current = CBM?.BillBMModes;
                    if (current.Count > 0)
                    {
                        foreach (var BillBMMode in current)
                        {
                            if (BillValuePart(BillBMMode) != billID)
                            {
                                newlist.AddDistinct(BillBMMode);
                            }
                        }
                    }
                    current.Clear();
                }
                newlist.AddDistinct(NewBillBMMode(billID, mode));

                CBM.BillBMModes = newlist;
                //newlist.Clear();
            }
        }

        public static void CheckBillBMValues(CompBestMix CBM, Thing billGiver, List<string> BillModes)
        {
            if (BillModes != null)
            {
                if (BillModes.Count > 0)
                {
                    var newBillModes = new List<string>();
                    var billIDs = new List<string>();
                    BillStack billStack = (billGiver as IBillGiver)?.BillStack;
                    if (billStack != null)
                    {
                        List<Bill> bills = billStack?.Bills;
                        if (bills.Count > 0)
                        {
                            foreach (Bill bill in bills)
                            {
                                var id = bill?.GetUniqueLoadID();
                                if (id != null)
                                {
                                    billIDs.AddDistinct<string>(id);
                                }
                            }
                        }
                    }

                    foreach (var BillMode in BillModes)
                    {
                        if (billIDs.Contains(BillValuePart(BillMode)))
                        {
                            newBillModes.AddDistinct<string>(BillMode);
                        }
                    }

                    CBM.BillBMModes = newBillModes;
                    //newBillModes.Clear();
                    //billIDs.Clear();
                }
            }
            else
            {
                CBM.BillBMModes = new List<string>();
            }
        }

        public static string NewBillBMMode(string id, string mode)
        {
            return id + ";" + mode;
        }

        public static string BillValuePart(string value)
        {
            char[] divider = { ';' };
            var segments = value.Split(divider);
            return segments[0];
        }

        public static string ModeValuePart(string value)
        {
            char[] divider = { ';' };
            var segments = value.Split(divider);
            return segments[1];
        }
    }
}
