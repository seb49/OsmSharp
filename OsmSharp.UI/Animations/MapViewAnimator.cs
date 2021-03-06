﻿// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2013 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Threading;
using OsmSharp.Math.Geo;
using OsmSharp.Units.Angle;

namespace OsmSharp.UI.Animations
{
    /// <summary>
    /// Represents an animator responsible for animating movements on IMapViews.
    /// </summary>
    public class MapViewAnimator
    {
        /// <summary>
        /// Holds the map view.
        /// </summary>
        private readonly IMapView _mapView;

        /// <summary>
        /// Holds the default time span.
        /// </summary>
        private readonly TimeSpan _defaultTimeSpan;

        /// <summary>
        /// Holds the minimum allowed timespan.
        /// </summary>
		private readonly TimeSpan _minimumTimeSpan = new TimeSpan(0, 0, 0, 0, 50);

//        /// <summary>
//        /// Holds a synchronization object for the timer elapsed event.
//        /// </summary>
//        private static object TimerElapsedSync = new object();

        /// <summary>
        /// Creates a new MapView Animator.
        /// </summary>
        public MapViewAnimator(IMapView mapView)
        {
            if (mapView == null) { throw new ArgumentNullException("mapView"); }

            _mapView = mapView;
            _defaultTimeSpan = new TimeSpan(0, 0, 1);
        }

        /// <summary>
        /// Starts an animation to a given zoom level.
        /// </summary>
        /// <param name="zoomLevel"></param>
        public void Start(float zoomLevel)
        {
            this.Start(_mapView.MapCenter, zoomLevel, _mapView.MapTilt, _defaultTimeSpan);
        }

        /// <summary>
        /// Starts an animation to a given zoom level that will take the given timespan.
        /// </summary>
        /// <param name="zoomLevel"></param>
        /// <param name="time"></param>
        public void Start(float zoomLevel, TimeSpan time)
        {
            this.Start(_mapView.MapCenter, zoomLevel, _mapView.MapTilt, time);
        }

        /// <summary>
        /// Starts an animation to a given map tilt.
        /// </summary>
        /// <param name="mapTilt"></param>
        public void Start(Degree mapTilt)
        {
            this.Start(_mapView.MapCenter, _mapView.MapZoom, mapTilt, _defaultTimeSpan);
        }

        /// <summary>
        /// Starts an animation to a given map tilt that will take the given timespan.
        /// </summary>
        /// <param name="mapTilt"></param>
        /// <param name="time"></param>
        public void Start(Degree mapTilt, TimeSpan time)
        {
            this.Start(_mapView.MapCenter, _mapView.MapZoom, mapTilt, time);
        }

        /// <summary>
        /// Starts an animation to a given map center.
        /// </summary>
        /// <param name="center"></param>
        public void Start(GeoCoordinate center)
        {
            this.Start(center, _mapView.MapZoom, _mapView.MapTilt, _defaultTimeSpan);
        }

        /// <summary>
        /// Starts an animation to a given map center that will take the given timespan.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="time"></param>
        public void Start(GeoCoordinate center, TimeSpan time)
        {
            this.Start(center, _mapView.MapZoom, _mapView.MapTilt, time);
        }

        /// <summary>
        /// Starts a animation to a given map center and given zoom level.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="zoomLevel"></param>
        public void Start(GeoCoordinate center, float zoomLevel)
        {
            this.Start(center, zoomLevel, _mapView.MapTilt, _defaultTimeSpan);
        }

        /// <summary>
        /// Starts an animation to the given parameters.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="zoomLevel"></param>
        /// <param name="time"></param>
        public void Start(GeoCoordinate center, float zoomLevel, TimeSpan time)
        {
            this.Start(center, zoomLevel, _mapView.MapTilt, time);
        }

        /// <summary>
        /// Holds the target zoom level.
        /// </summary>
        private float _targetZoom;

        /// <summary>
        /// Holds the target tilt.
        /// </summary>
        private Degree _targetTilt;

        /// <summary>
        /// Holds the target center.
        /// </summary>
        private GeoCoordinate _targetCenter;

        /// <summary>
        /// Holds the step zoom level.
        /// </summary>
        private float _stepZoom;

        /// <summary>
        /// Holds the step tilt.
        /// </summary>
        private double _stepTilt;

        /// <summary>
        /// Holds the step center.
        /// </summary>
        private GeoCoordinate _stepCenter;

        /// <summary>
        /// Holds the timer.
        /// </summary>
        private Timer _timer;

		/// <summary>
		/// Holds the timer status.
		/// </summary>
		private AnimatorStatus _timerStatus;

        /// <summary>
        /// Holds the current step.
        /// </summary>
        private int _currentStep;

        /// <summary>
        /// Holds the maximum number of steps.
        /// </summary>
        private int _maxSteps;

        /// <summary>
        /// Starts an animation to the given parameters.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="zoomLevel"></param>
        /// <param name="mapTilt"></param>
        /// <param name="time"></param>
        public void Start(GeoCoordinate center, float zoomLevel, Degree mapTilt, TimeSpan time)
		{
			// stop the previous timer.
			if (_timer != null)
			{ // timer exists, it might be active disable it immidiately.
				// cancel previous status.
				_timerStatus.Cancelled = true;
				_timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
				_timer.Dispose();
			}

			// set the targets.
			_targetCenter = center;
			_targetTilt = mapTilt;
			_targetZoom = zoomLevel;

			// calculate the animation steps.
			_maxSteps = (int)System.Math.Round((double)time.TotalMilliseconds / (double)_minimumTimeSpan.TotalMilliseconds, 0);
			_currentStep = 0;
			_stepCenter = new GeoCoordinate(
				(_targetCenter.Latitude - _mapView.MapCenter.Latitude) / _maxSteps,
				(_targetCenter.Longitude - _mapView.MapCenter.Longitude) / _maxSteps);
			_stepZoom = (float)((_targetZoom - _mapView.MapZoom) / _maxSteps);

			// calculate the map tilt, make sure it turns along the smallest corner.
			double diff = _targetTilt.SmallestDifference(_mapView.MapTilt);
			OsmSharp.Logging.Log.TraceEvent("MapViewAnimator", OsmSharp.Logging.TraceEventType.Information, diff.ToString());
				_stepTilt = (diff / _maxSteps);

			OsmSharp.Logging.Log.TraceEvent("MapViewAnimator", OsmSharp.Logging.TraceEventType.Verbose,
				string.Format("Started new animation with steps z:{0} t:{1} c:{2} to z:{3} t:{4} c:{5} from z:{6} t:{7} c:{8}.",
					_stepZoom, _stepTilt, _stepCenter.ToString(), 
					_targetZoom, _targetTilt, _targetCenter.ToString(), 
					_mapView.MapZoom, _mapView.MapTilt, _mapView.MapCenter.ToString()));

			// disable auto invalidate.
			_mapView.RegisterAnimator(this);

			// start the timer.
			// create a new timer.
			_timerStatus = new AnimatorStatus();
			_timer = new Timer(new TimerCallback(_timer_Elapsed), _timerStatus, 0, (int)_minimumTimeSpan.TotalMilliseconds);
		}

        /// <summary>
        /// Stops the animation.
        /// </summary>
        public void Stop()
		{
			if (_timer != null)
			{ // disable timer.
				// cancel previous status.
				_timerStatus.Cancelled = true;
				_timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
				_timer.Dispose();
			}

			// unregister this animator, it has been stopped.
			_mapView.RegisterAnimator(null);
		}

        /// <summary>
        /// The timer has elapsed.
        /// </summary>
        /// <param name="sender"></param>
        void _timer_Elapsed(object sender)
		{
			var status = sender as AnimatorStatus;
			if (status.Cancelled)
			{ // check status when cancelled return.
				return;
			} 

			_currentStep++;
			if (_currentStep < _maxSteps)
			{ // there is still need for a change.
				GeoCoordinate center = new GeoCoordinate(// update center.
					                       (_mapView.MapCenter.Latitude + _stepCenter.Latitude),
					                       (_mapView.MapCenter.Longitude + _stepCenter.Longitude));
				float mapZoom = _mapView.MapZoom + _stepZoom; // update zoom.
				Degree mapTilt = _mapView.MapTilt + _stepTilt; // update tilt.

				_mapView.SetMapView(center, mapTilt, mapZoom);
				return;
			}
			else if (_currentStep == _maxSteps)
			{ // this is the last step.
				// enable auto invalidate.
				_mapView.RegisterAnimator(null);

				// stop the timer, animation has been finished.
				_timerStatus.Cancelled = true;
				_timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
				_timer.Dispose();

				// set mapview.
				_mapView.SetMapView(_targetCenter, _targetTilt, _targetZoom);
			}
			else
			{ // hmm animator should be finished?
				// enable auto invalidate.
				_mapView.RegisterAnimator(null);

				// stop the timer, animation has been finished.
				_timerStatus.Cancelled = true;
				_timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
				_timer.Dispose();
			}
		}

		private class AnimatorStatus
		{
			public AnimatorStatus()
			{
				this.Cancelled = false;
			}

			public bool Cancelled {
				get;
				set;
			}
		}
    }
}