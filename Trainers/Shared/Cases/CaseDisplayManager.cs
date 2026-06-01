using CubeForge.Trainers.Common;
using CubeForge.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CubeForge.Trainers.Shared
{
    public sealed class CaseDisplayManager<TCase> where TCase : ICaseInfo
    {
        private readonly CaseSelection<TCase> caseSelection;
        private readonly ImageHelper imageHelper;
        private readonly AlgorithmPlaybackManager playback;
        private readonly ModeManager mode;
        private readonly Func<TCase, string> getSetupMoves;
        private readonly Func<TCase, string> getDefaultAlgorithmMoves;

        public TCase? SelectedCase { get; private set; }

        public CaseDisplayManager(CaseSelection<TCase> caseSelection, ImageHelper imageHelper, AlgorithmPlaybackManager playback, ModeManager mode, Func<TCase, string> getSetupMoves, Func<TCase, string> getDefaultAlgorithmMoves)
        {
            this.caseSelection = caseSelection;
            this.imageHelper = imageHelper;
            this.playback = playback;
            this.mode = mode;
            this.getSetupMoves = getSetupMoves;
            this.getDefaultAlgorithmMoves = getDefaultAlgorithmMoves;
        }

        public bool ShowCase(CaseViewModel trainerCase)
        {
            TCase? selectedCase = caseSelection.AllCases.FirstOrDefault(c => c.Number == trainerCase.Number);

            if (selectedCase == null) return false;

            SelectedCase = selectedCase;
            string algorithmMoves = trainerCase.SelectedAlgorithm?.Moves ?? getDefaultAlgorithmMoves(selectedCase);

            playback.SetCase(trainerCase.Name, trainerCase.Group, getSetupMoves(selectedCase), algorithmMoves);
            mode.SetAlgorithmImage(imageHelper.LoadImage(trainerCase.PreviewImage));
            playback.Reset();
            return true;
        }
    }
}
