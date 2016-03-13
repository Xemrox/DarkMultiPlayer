using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DarkMultiPlayer {
    public class EditSync {
        private static EditSync singleton;
        private bool registered = false;

        public static EditSync fetch {
            get {
                return singleton;
            }
        }

        public void RegisterGameHooks() {
            if(!registered) {
                registered = false;

                GameEvents.onEditorLoad.Add(this.OnEditorLoad);
                GameEvents.onEditorPartEvent.Add(this.OnEditorPartEvent);
                GameEvents.onEditorRedo.Add(this.OnEditorRedo);
                GameEvents.onEditorRestart.Add(this.OnEditorRestart);
                GameEvents.onEditorScreenChange.Add(this.OnEditorScreenChange);
                GameEvents.onEditorShipModified.Add(this.OnEditorShipModified);
                GameEvents.onEditorShowPartList.Add(this.OnEditorShowPartList);
                GameEvents.onEditorSnapModeChange.Add(this.OnEditorSnapModeChange);
                GameEvents.onEditorSymmetryCoordsChange.Add(this.OnEditorSymmetryCoordsChange);
                GameEvents.onEditorSymmetryMethodChange.Add(this.OnEditorSymmetryMethodChange);
                GameEvents.onEditorSymmetryModeChange.Add(this.OnEditorSymmetryModeChange);
                GameEvents.onEditorUndo.Add(this.OnEditorUndo);
            }
        }

        private void UnregisterGameHooks() {
            if (registered) {
                registered = false;

                GameEvents.onEditorLoad.Remove(this.OnEditorLoad);
                GameEvents.onEditorPartEvent.Remove(this.OnEditorPartEvent);
                GameEvents.onEditorRedo.Remove(this.OnEditorRedo);
                GameEvents.onEditorRestart.Remove(this.OnEditorRestart);
                GameEvents.onEditorScreenChange.Remove(this.OnEditorScreenChange);
                GameEvents.onEditorShipModified.Remove(this.OnEditorShipModified);
                GameEvents.onEditorShowPartList.Remove(this.OnEditorShowPartList);
                GameEvents.onEditorSnapModeChange.Remove(this.OnEditorSnapModeChange);
                GameEvents.onEditorSymmetryCoordsChange.Remove(this.OnEditorSymmetryCoordsChange);
                GameEvents.onEditorSymmetryMethodChange.Remove(this.OnEditorSymmetryMethodChange);
                GameEvents.onEditorSymmetryModeChange.Remove(this.OnEditorSymmetryModeChange);
                GameEvents.onEditorUndo.Remove(this.OnEditorUndo);
            }
        }

        public static void Reset() {
            lock (Client.eventLock) {
                if (singleton != null) {
                    if (singleton.registered) {
                        singleton.UnregisterGameHooks();
                    }
                }
                singleton = new EditSync();
            }
        }

        private void OnEditorUndo(ShipConstruct data) {
            ConfigNode n = data.SaveShip();
        }

        private void OnEditorSymmetryModeChange(int data) {
            
        }

        private void OnEditorSymmetryMethodChange(SymmetryMethod method) {
            NetworkWorker.fetch.SendSymmetryMethodChange(method);
        }

        private void OnEditorSymmetryCoordsChange(Space sp) {
            NetworkWorker.fetch.SendSymmetryCoordsChange(sp);
        }

        private void OnEditorSnapModeChange(bool mode) {
            NetworkWorker.fetch.SendSnapModeChange(mode);
        }

        private void OnEditorShowPartList() {
             
        }

        private void OnEditorShipModified(ShipConstruct data) {
            
        }

        private void OnEditorScreenChange(EditorScreen screen) {
            NetworkWorker.fetch.SendEditorScreenChange(screen);
        }

        private void OnEditorRestart() {
            
        }

        private void OnEditorRedo(ShipConstruct sp) {
            NetworkWorker.fetch.SendEditorRedo(sp);
        }

        private void OnEditorPartEvent(ConstructionEventType cet, Part p) {
            //Main editor handling!
            NetworkWorker.fetch.SendEditorPartEvent(cet, p);
        }

        private void OnEditorLoad(ShipConstruct sp, CraftBrowser.LoadType lt) {
           
        }

    }
}
