using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Android.Views.View;

namespace SlideToShutDown
{
    public class SliderManager
    {
        private const double        _hopTeleportLength = 1;
        private const int           _hopTeleportTime = 100;
        private readonly long       _defaultAnimationTime;

        private readonly View       _slider;
        private readonly double        _screenHeight;
        private readonly double     _startPosition;
        private readonly double     _lowGravityBound;
        private readonly double     _highGravityBound;
        private readonly long       _timeOut;

        // All the parameters below refers to the TOP of the slider.
        private readonly double     _startY;
        private readonly double     _lowYBound;
        private readonly double     _highYBound;

        private bool                _isDragging;
        private double              _currentY;
        private double              _dragY;

        private Task                    _hopper;
        private double                  _hopRange;
        private CancellationTokenSource _hopperToken;

        private Task                    _countDownTask;
        private CancellationTokenSource _countDownToken;
        private DateTime                _timeStamp;

        public event EventHandler Slided;
        public event EventHandler Canceled;

        /// <summary>
        /// Creates a vertical slider manager.
        /// </summary>
        /// <param name="slider">The view, intended as the slider</param>
        /// <param name="screenHeight">Height of the phone screen</param>
        /// <param name="position">The initial position of the bottom of the slider, from 0.0 (hidden) to 1.0 (full screen) </param>
        /// <param name="lowGravityBound">A position, from 0.0 to 1.0. If the bottom of the slider gets above this, the slider will automatically hide.</param>
        /// <param name="highGravityBound">A position, from 0.0 to 1.0. If the bottom of the slider gets below this, the slider will automatically fill the screen.</param>
        /// <param name="timeOut">The timeout, in milliseconds before the slider hides.</param>
        public SliderManager(View slider, double screenHeight, double position, double lowGravityBound, double highGravityBound, long timeOut)
        {
            _slider = slider;
            _screenHeight = screenHeight;
            _startPosition = position;
            _lowGravityBound = lowGravityBound;
            _highGravityBound = highGravityBound;

            _defaultAnimationTime = slider.Animate().Duration;

            _lowYBound = -screenHeight;
            _highYBound = 0;
            // x percent shown => 1.0 - x percent hidden.
            _startY = -screenHeight * (1.0 - position);
            _timeOut = timeOut;

            Teleport(_lowYBound);
        }

        public void Show(double? hopRange = null)
        {
            Animate(_startY);
            _slider.Touch += SliderTouched;
            if (hopRange != null)
            {
                HopAsync(hopRange.Value);
            }
            StartCountDownAsync();
        }

        public void Hide()
        {
            HideAsync();
        }

        public async Task HideAsync()
        {
            await StopHopAsync();
            await StopCountDownAsync();
            _slider.Touch -= SliderTouched;
            Animate(_lowYBound);
            await Task.Delay((int)_defaultAnimationTime);
        }

        private async Task StopCountDownAsync()
        {
            if (_countDownTask != null)
            {
                _countDownToken.Cancel();
                await _countDownTask;
                _countDownToken.Dispose();
                _countDownTask = null;
                _countDownToken = null;
            }
        }

        private void StartCountDownAsync()
        {
            Func<Task> func = async () =>
            {
                _countDownToken = new CancellationTokenSource();
                _timeStamp = DateTime.Now;
                while ((DateTime.Now - _timeStamp).TotalMilliseconds <= _timeOut)
                {
                    if (_countDownToken.IsCancellationRequested)
                    {
                        return;
                    }
                    await Task.Delay(100);
                }
                if (_countDownToken.IsCancellationRequested)
                {
                    return;
                }
                _countDownTask = null;
                _countDownToken?.Dispose();
                _countDownToken = null;
                await HideAsync();
                Canceled?.Invoke(this, EventArgs.Empty);
            };
            _countDownTask = func();
        }

        private void Animate(double y)
        {
            _currentY = y;
            var animator = _slider.Animate().Y((float)y);
        }

        private void Teleport(double y)
        {
            _currentY = y;
            _slider.SetY((float)y);
        }

        private void HopAsync(double hopRange)
        {
            _hopperToken = new CancellationTokenSource();
            _hopRange = hopRange;
            var token = _hopperToken.Token;
            _hopper = Task.Run(() =>
            {
                bool isUp = false;
                while (true)
                {
                    if (token.IsCancellationRequested)
                    {
                        System.Diagnostics.Debug.WriteLine("Cancelled.");
                        return;
                    }
                    if (!_isDragging)
                    {
                        Teleport(_currentY + (isUp ? -1 : 1) * _hopTeleportLength);
                        if (Math.Clamp(_currentY, _startY, Math.Min(_startY + hopRange, _highYBound)) != _currentY)
                        {
                            isUp = !isUp;
                        }
                    }
                    Thread.Sleep(_hopTeleportTime);
                }
            }, token);
        }

        private async Task StopHopAsync()
        {
            if (_hopper != null)
            {
                _hopperToken.Cancel();
                await _hopper;
                _hopper = null;
                _hopperToken.Dispose();
                _hopperToken = null;
            }
        }

        private void DragBegin(double dragY)
        {
            StopCountDownAsync();
            _isDragging = true;
            _dragY = dragY;
        }

        private void DragMove(double newY)
        {
            var diff = newY - _dragY;
            Teleport(Math.Clamp(_currentY + diff, _lowYBound, _highYBound));
            if (_currentY == _highYBound)
            {
                _slider.Touch -= SliderTouched;
                Slided?.Invoke(this, EventArgs.Empty);
                _isDragging = false;
            }
            if (_currentY == _lowYBound)
            {
                _slider.Touch -= SliderTouched;
                Canceled?.Invoke(this, EventArgs.Empty);
                _isDragging = false;
            }
            _dragY = newY;
        }

        private void DragEnd()
        {
            double distance = (_currentY - _lowYBound) / _screenHeight;

            System.Diagnostics.Debug.WriteLine(distance);

            if (distance <= _lowGravityBound)
            {
                // Hide, cancel.
                Task.Run(async () =>
                {
                    await HideAsync();
                    Canceled?.Invoke(this, EventArgs.Empty);
                    _isDragging = false;
                });
            }
            else if (distance >= _highGravityBound)
            {
                //Freeze.
                _slider.Touch -= SliderTouched;

                // Do action.
                Animate(_highYBound);
                Task.Run(async () =>
                {
                    await Task.Delay((int)_defaultAnimationTime);
                    _isDragging = false;
                });
                Slided?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                Animate(_startY);
                Task.Run(async () =>
                {
                    StartCountDownAsync();
                    await Task.Delay((int)_defaultAnimationTime);
                    _isDragging = false;
                });
            }
        }

        private void SliderTouched(object sender, TouchEventArgs args)
        {
            var action = (MotionEventActions)((int)args.Event.Action & (int)MotionEventActions.Mask);
            switch (action)
            {
                case MotionEventActions.Down:
                    if (!_isDragging)
                    {
                        DragBegin(args.Event.RawY);
                    }
                break;
                case MotionEventActions.Move:
                    DragMove(args.Event.RawY);
                break;
                case MotionEventActions.Up:
                    DragEnd();
                break;
            }
        }
    }
}