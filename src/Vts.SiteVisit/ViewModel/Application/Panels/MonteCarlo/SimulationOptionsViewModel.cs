﻿using System;
using System.Collections.Generic;
using System.Linq;
using Vts.MonteCarlo;

namespace Vts.SiteVisit.ViewModel
{
    public class SimulationOptionsViewModel : BindableObject
    {
        private SimulationOptions _simulationOptions;
        private OptionViewModel<AbsorptionWeightingType> _absorptionWeightingTypeVM;
        private OptionViewModel<RandomNumberGeneratorType> _randomNumberGeneratorTypeVM;
        private OptionViewModel<PhaseFunctionType> _phaseFunctionTypeVM;
        
        public SimulationOptionsViewModel(SimulationOptions options)
        {
            _simulationOptions = options; // use the property to invoke the appropriate change notification
            
#if WHITELIST 
            _absorptionWeightingTypeVM = new OptionViewModel<AbsorptionWeightingType>("Absorption Weighting Type:", false, _simulationOptions.AbsorptionWeightingType, WhiteList.ScatteringTypes);
            _randomNumberGeneratorTypeVM = new OptionViewModel<RandomNumberGeneratorType>("Random Number Generator Type:", false, _simulationOptions.RandomNumberGeneratorType, WhiteList.RandomNumberGeneratorTypes);
            _phaseFunctionTypeVM = new OptionViewModel<PhaseFunctionType>("Phase Function Type:", false, _simulationOptions.PhaseFunctionType, WhiteList.PhaseFunctionTypes);
#else
            _absorptionWeightingTypeVM = new OptionViewModel<AbsorptionWeightingType>("Absorption Weighting Type:", false, _simulationOptions.AbsorptionWeightingType);
            _randomNumberGeneratorTypeVM = new OptionViewModel<RandomNumberGeneratorType>("Random Number Generator:", false, _simulationOptions.RandomNumberGeneratorType);
            _phaseFunctionTypeVM = new OptionViewModel<PhaseFunctionType>("Phase Function Type:", false, _simulationOptions.PhaseFunctionType);
#endif
            _absorptionWeightingTypeVM.PropertyChanged += (sender, args) =>
                _simulationOptions.AbsorptionWeightingType = _absorptionWeightingTypeVM.SelectedValue;
            _randomNumberGeneratorTypeVM.PropertyChanged += (sender, args) =>
                _simulationOptions.RandomNumberGeneratorType = _randomNumberGeneratorTypeVM.SelectedValue;
            _phaseFunctionTypeVM.PropertyChanged += (sender, args) =>
                _simulationOptions.PhaseFunctionType = _phaseFunctionTypeVM.SelectedValue;
        }

        public SimulationOptionsViewModel() : this(new SimulationOptions())
        {
        }

        public SimulationOptions SimulationOptions
        {
            get { return _simulationOptions; }
            set
            {
                _simulationOptions = value;
                OnPropertyChanged("SimulationOptions");
            }
        }

        public int Seed
        {
            get { return _simulationOptions.Seed; }
            set
            {
                _simulationOptions.Seed = value;
                OnPropertyChanged("Seed");
            }
        }

        public bool TallySecondMoment
        {
            get { return _simulationOptions.TallySecondMoment; }
            set
            {
                _simulationOptions.TallySecondMoment = value;
                OnPropertyChanged("TallySecondMoment");
            }
        }

        public bool TrackStatistics
        {
            get { return _simulationOptions.TrackStatistics; }
            set
            {
                _simulationOptions.TrackStatistics = value;
                OnPropertyChanged("TrackStatistics");
            }
        }

        public int SimulationIndex
        {
            get { return _simulationOptions.SimulationIndex; }
            set
            {
                _simulationOptions.SimulationIndex = value;
                OnPropertyChanged("TrackStatistics");
            }
        }

        //public RandomNumberGeneratorType RandomNumberGeneratorType
        //{
        //    get { return _simulationOptions.RandomNumberGeneratorType; }
        //    set
        //    {
        //        _simulationOptions.RandomNumberGeneratorType = value;
        //        OnPropertyChanged("RandomNumberGeneratorType");
        //    }
        //}

        public OptionViewModel<AbsorptionWeightingType> AbsorptionWeightingTypeVM
        {
            get { return _absorptionWeightingTypeVM; }
            set
            {
                _absorptionWeightingTypeVM = value;
                OnPropertyChanged("AbsorptionWeightingTypeVM");
            }
        }

        public OptionViewModel<RandomNumberGeneratorType> RandomNumberGeneratorTypeVM
        {
            get { return _randomNumberGeneratorTypeVM; }
            set
            {
                _randomNumberGeneratorTypeVM = value;
                OnPropertyChanged("RandomNumberGeneratorTypeVM");
            }
        }

        public OptionViewModel<PhaseFunctionType> PhaseFunctionTypeVM
        {
            get { return _phaseFunctionTypeVM; }
            set
            {
                _phaseFunctionTypeVM = value;
                OnPropertyChanged("PhaseFunctionTypeVM");
            }
        }
    }
}