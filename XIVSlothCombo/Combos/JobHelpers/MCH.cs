﻿using System.Linq;
using Dalamud.Game.ClientState.JobGauge.Types;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using XIVSlothCombo.Combos.JobHelpers.Enums;
using XIVSlothCombo.Combos.PvE;
using XIVSlothCombo.Data;
using static XIVSlothCombo.Combos.PvE.MCH;
using static XIVSlothCombo.CustomComboNS.Functions.CustomComboFunctions;

namespace XIVSlothCombo.Combos.JobHelpers;

internal class MCH
{
    // MCH Gauge & Extensions
    public static MCHOpenerLogic MCHOpener = new();

    public static float GCD = GetCooldown(OriginalHook(SplitShot)).CooldownTotal;
    public static float heatblastRC = GetCooldown(Heatblast).CooldownTotal;
    public static MCHGauge Gauge = GetJobGauge<MCHGauge>();

    public static bool drillCD = !LevelChecked(Drill) || (!TraitLevelChecked(Traits.EnhancedMultiWeapon) &&
                                                          GetCooldownRemainingTime(Drill) > heatblastRC * 6) ||
                                 (TraitLevelChecked(Traits.EnhancedMultiWeapon) &&
                                  GetRemainingCharges(Drill) < GetMaxCharges(Drill) &&
                                  GetCooldownRemainingTime(Drill) > heatblastRC * 6);

    public static bool anchorCD = !LevelChecked(AirAnchor) ||
                                  (LevelChecked(AirAnchor) && GetCooldownRemainingTime(AirAnchor) > heatblastRC * 6);

    public static bool sawCD = !LevelChecked(Chainsaw) ||
                               (LevelChecked(Chainsaw) && GetCooldownRemainingTime(Chainsaw) > heatblastRC * 6);

    public static bool interruptReady => ActionReady(All.HeadGraze) && CanInterruptEnemy() &&
                                         CanDelayedWeave(ActionWatching.LastWeaponskill);

    public static int BSUsed => ActionWatching.CombatActions.Count(x => x == BarrelStabilizer);

    internal class MCHOpenerLogic
    {
        private OpenerState currentState = OpenerState.PrePull;

        public uint OpenerStep;

        public uint PrePullStep;

        private static uint OpenerLevel => 100;

        public static bool LevelChecked => LocalPlayer.Level >= OpenerLevel;

        private static bool CanOpener => HasCooldowns() && LevelChecked;

        public OpenerState CurrentState
        {
            get => currentState;
            set
            {
                if (value != currentState)
                {
                    if (value == OpenerState.PrePull) Svc.Log.Debug("Entered PrePull Opener");
                    if (value == OpenerState.InOpener) OpenerStep = 1;

                    if (value == OpenerState.OpenerFinished || value == OpenerState.FailedOpener)
                    {
                        if (value == OpenerState.FailedOpener)
                            Svc.Log.Information($"Opener Failed at step {OpenerStep}");

                        ResetOpener();
                    }
                    if (value == OpenerState.OpenerFinished) Svc.Log.Information("Opener Finished");

                    currentState = value;
                }
            }
        }

        private static bool HasCooldowns()
        {
            if (GetRemainingCharges(CheckMate) < 3)
                return false;

            if (GetRemainingCharges(DoubleCheck) < 3)
                return false;

            if (!ActionReady(Chainsaw))
                return false;

            if (!ActionReady(Wildfire))
                return false;

            if (!ActionReady(BarrelStabilizer))
                return false;

            if (!ActionReady(Excavator))
                return false;

            if (!ActionReady(FullMetalField))
                return false;

            return true;
        }

        private bool DoPrePullSteps(ref uint actionID)
        {
            if (!LevelChecked)
                return false;

            if (CanOpener && PrePullStep == 0) PrePullStep = 1;

            if (!HasCooldowns()) PrePullStep = 0;

            if (CurrentState == OpenerState.PrePull && PrePullStep > 0)
            {
                if (HasEffect(Buffs.Reassembled) && PrePullStep == 1) CurrentState = OpenerState.InOpener;
                else if (PrePullStep == 1) actionID = Reassemble;

                if (ActionWatching.CombatActions.Count > 2 && InCombat())
                    CurrentState = OpenerState.FailedOpener;

                return true;
            }
            PrePullStep = 0;

            return false;
        }

        private bool DoOpener(ref uint actionID)
        {
            if (!LevelChecked)
                return false;

            if (currentState == OpenerState.InOpener)
            {
                if (WasLastAction(AirAnchor) && OpenerStep == 1) OpenerStep++;
                else if (OpenerStep == 1) actionID = AirAnchor;

                if (WasLastAction(CheckMate) && OpenerStep == 2) OpenerStep++;
                else if (OpenerStep == 2) actionID = CheckMate;

                if (WasLastAction(DoubleCheck) && OpenerStep == 3) OpenerStep++;
                else if (OpenerStep == 3) actionID = DoubleCheck;

                if (WasLastAction(Drill) && OpenerStep == 4) OpenerStep++;
                else if (OpenerStep == 4) actionID = Drill;

                if (WasLastAction(BarrelStabilizer) && OpenerStep == 5) OpenerStep++;
                else if (OpenerStep == 5) actionID = BarrelStabilizer;

                if (WasLastAction(Chainsaw) && OpenerStep == 6) OpenerStep++;
                else if (OpenerStep == 6) actionID = Chainsaw;

                if (WasLastAction(Excavator) && OpenerStep == 7) OpenerStep++;
                else if (OpenerStep == 7) actionID = Excavator;

                if (WasLastAction(AutomatonQueen) && OpenerStep == 8) OpenerStep++;
                else if (OpenerStep == 8) actionID = AutomatonQueen;

                if (WasLastAction(Reassemble) && OpenerStep == 9) OpenerStep++;
                else if (OpenerStep == 9) actionID = Reassemble;

                if (WasLastAction(Drill) && OpenerStep == 10) OpenerStep++;
                else if (OpenerStep == 10) actionID = Drill;

                if (WasLastAction(CheckMate) && OpenerStep == 11) OpenerStep++;
                else if (OpenerStep == 11) actionID = CheckMate;

                if (WasLastAction(Wildfire) && OpenerStep == 12) OpenerStep++;
                else if (OpenerStep == 12) actionID = Wildfire;

                if (WasLastAction(FullMetalField) && OpenerStep == 13) OpenerStep++;
                else if (OpenerStep == 13) actionID = FullMetalField;

                if (WasLastAction(DoubleCheck) && OpenerStep == 14) OpenerStep++;
                else if (OpenerStep == 14) actionID = DoubleCheck;

                if (WasLastAction(Hypercharge) && OpenerStep == 15) OpenerStep++;
                else if (OpenerStep == 15) actionID = Hypercharge;

                if (WasLastAction(BlazingShot) && OpenerStep == 16) OpenerStep++;
                else if (OpenerStep == 16) actionID = BlazingShot;

                if (WasLastAction(CheckMate) && OpenerStep == 17) OpenerStep++;
                else if (OpenerStep == 17) actionID = CheckMate;

                if (WasLastAction(BlazingShot) && OpenerStep == 18) OpenerStep++;
                else if (OpenerStep == 18) actionID = BlazingShot;

                if (WasLastAction(DoubleCheck) && OpenerStep == 19) OpenerStep++;
                else if (OpenerStep == 19) actionID = DoubleCheck;

                if (WasLastAction(BlazingShot) && OpenerStep == 20) OpenerStep++;
                else if (OpenerStep == 20) actionID = BlazingShot;

                if (WasLastAction(CheckMate) && OpenerStep == 21) OpenerStep++;
                else if (OpenerStep == 21) actionID = CheckMate;

                if (WasLastAction(BlazingShot) && OpenerStep == 22) OpenerStep++;
                else if (OpenerStep == 22) actionID = BlazingShot;

                if (WasLastAction(DoubleCheck) && OpenerStep == 23) OpenerStep++;
                else if (OpenerStep == 23) actionID = DoubleCheck;

                if (WasLastAction(BlazingShot) && OpenerStep == 24) OpenerStep++;
                else if (OpenerStep == 24) actionID = BlazingShot;

                if (WasLastAction(CheckMate) && OpenerStep == 25) OpenerStep++;
                else if (OpenerStep == 25) actionID = CheckMate;

                if (WasLastAction(Drill) && OpenerStep == 26) OpenerStep++;
                else if (OpenerStep == 26) actionID = Drill;

                if (WasLastAction(DoubleCheck) && OpenerStep == 27) OpenerStep++;
                else if (OpenerStep == 27) actionID = DoubleCheck;

                if (WasLastAction(CheckMate) && OpenerStep == 28) OpenerStep++;
                else if (OpenerStep == 28) actionID = CheckMate;

                if (WasLastAction(HeatedSplitShot) && OpenerStep == 29) OpenerStep++;
                else if (OpenerStep == 29) actionID = HeatedSplitShot;

                if (WasLastAction(DoubleCheck) && OpenerStep == 30) OpenerStep++;
                else if (OpenerStep == 30) actionID = DoubleCheck;

                if (WasLastAction(HeatedSlugShot) && OpenerStep == 31) OpenerStep++;
                else if (OpenerStep == 31) actionID = HeatedSlugShot;

                if (WasLastAction(HeatedCleanShot) && OpenerStep == 32) CurrentState = OpenerState.OpenerFinished;
                else if (OpenerStep == 32) actionID = HeatedCleanShot;

                if (ActionWatching.TimeSinceLastAction.TotalSeconds >= 5)
                    CurrentState = OpenerState.FailedOpener;

                if (((actionID == CheckMate && GetRemainingCharges(CheckMate) < 3) ||
                     (actionID == Chainsaw && IsOnCooldown(Chainsaw)) ||
                     (actionID == Wildfire && IsOnCooldown(Wildfire)) ||
                     (actionID == BarrelStabilizer && IsOnCooldown(BarrelStabilizer)) ||
                     (actionID == BarrelStabilizer && IsOnCooldown(Excavator)) ||
                     (actionID == BarrelStabilizer && IsOnCooldown(FullMetalField)) ||
                     (actionID == DoubleCheck && GetRemainingCharges(DoubleCheck) < 3)) &&
                    ActionWatching.TimeSinceLastAction.TotalSeconds >= 3)
                {
                    CurrentState = OpenerState.FailedOpener;

                    return false;
                }

                return true;
            }

            return false;
        }

        private void ResetOpener()
        {
            PrePullStep = 0;
            OpenerStep = 0;
        }

        public bool DoFullOpener(ref uint actionID)
        {
            if (!LevelChecked)
                return false;

            if (CurrentState == OpenerState.PrePull)
                if (DoPrePullSteps(ref actionID))
                    return true;

            if (CurrentState == OpenerState.InOpener)
                if (DoOpener(ref actionID))
                    return true;

            if (!InCombat())
            {
                ResetOpener();
                CurrentState = OpenerState.PrePull;
            }

            return false;
        }
    }

    internal static class MCHHelper
    {
        public static unsafe bool IsComboExpiring(float Times)
        {
            float GCD = GetCooldown(OriginalHook(SplitShot)).CooldownTotal * Times;

            return ActionManager.Instance()->Combo.Timer != 0 && ActionManager.Instance()->Combo.Timer < GCD;
        }

        public static bool UseQueen(MCHGauge gauge)
        {
            if (!ActionWatching.HasDoubleWeaved() && !gauge.IsOverheated && !HasEffect(Buffs.Wildfire) &&
                !JustUsed(OriginalHook(Heatblast)) && LevelChecked(OriginalHook(RookAutoturret)) &&
                gauge is { IsRobotActive: false, Battery: >= 50 })
            {
                if (LevelChecked(FullMetalField))
                {
                    //1min
                    if ((BSUsed == 1) & (gauge.Battery >= 90))
                        return true;

                    //even mins
                    if (BSUsed >= 2 && gauge.Battery == 100)
                        return true;

                    //odd mins 1st queen
                    if (BSUsed >= 2 && gauge is { Battery: 50, LastSummonBatteryPower: 100 })
                        return true;

                    //odd mins 2nd queen
                    if (BSUsed % 3 is 2 && gauge is { Battery: >= 60, LastSummonBatteryPower: 50 })
                        return true;

                    //odd mins 2nd queen
                    if (BSUsed % 3 is 0 && gauge is { Battery: >= 70, LastSummonBatteryPower: 50 })
                        return true;

                    //odd mins 2nd queen
                    if (BSUsed % 3 is 1 && gauge is { Battery: >= 80, LastSummonBatteryPower: 50 })
                        return true;
                }

                if (!LevelChecked(FullMetalField))
                    if (gauge.Battery == 100)
                        return true;

                if (!LevelChecked(BarrelStabilizer))
                    return true;
            }

            return false;
        }
    }
}