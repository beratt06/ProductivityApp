document.addEventListener("DOMContentLoaded", () => {
  document.querySelectorAll("select[data-selected]").forEach((select) => {
    select.value = select.dataset.selected;
  });

  document.querySelectorAll(".js-hover").forEach((element) => {
    element.addEventListener("mouseenter", () => element.classList.add("is-hovered"));
    element.addEventListener("mouseleave", () => element.classList.remove("is-hovered"));
  });

  document.querySelectorAll("[data-live-filter-target]").forEach((input) => {
    const table = document.querySelector(input.dataset.liveFilterTarget);
    if (!table) {
      return;
    }

    input.addEventListener("input", () => {
      const query = input.value.trim().toLowerCase();
      table.querySelectorAll("tbody tr").forEach((row) => {
        row.hidden = query.length > 0 && !row.textContent.toLowerCase().includes(query);
      });
    });
  });

  const timerDisplay = document.querySelector("#timerDisplay");
  const startButton = document.querySelector("#startTimer");
  const resetButton = document.querySelector("#resetTimer");
  const durationInput = document.querySelector("#durationMinutes");
  const completionForm = document.querySelector("#focusCompleteForm");
  const durationButtons = document.querySelectorAll("[data-minutes]");
  const customMinutesInput = document.querySelector("#customMinutes");
  const applyCustomDurationButton = document.querySelector("#applyCustomDuration");
  const timerRing = document.querySelector("#timerRing");
  const timerStatus = document.querySelector("#timerStatus");
  const dailyGoalInput = document.querySelector("#dailyGoalInput");
  const dailyGoalProgress = document.querySelector("#dailyGoalProgress");
  const dailyGoalLabel = document.querySelector("#dailyGoalLabel");
  const focusPanel = document.querySelector(".timer-panel");
  const soundToggle = document.querySelector("#soundToggle");
  const notifyToggle = document.querySelector("#notifyToggle");

  if (!timerDisplay || !startButton || !resetButton || !durationInput || !completionForm) {
    return;
  }

  let selectedMinutes = Number(durationInput.value);
  let totalSeconds = selectedMinutes * 60;
  let remainingSeconds = totalSeconds;
  let timerId = null;

  const setDuration = (minutes, keepActiveButton = false) => {
    selectedMinutes = Math.min(Math.max(Number(minutes) || 25, 1), 180);
    durationInput.value = selectedMinutes;
    totalSeconds = selectedMinutes * 60;
    remainingSeconds = totalSeconds;

    if (!keepActiveButton) {
      durationButtons.forEach((item) => item.classList.remove("active"));
    }

    const matchingButton = [...durationButtons].find((item) => Number(item.dataset.minutes) === selectedMinutes);
    if (matchingButton) {
      durationButtons.forEach((item) => item.classList.remove("active"));
      matchingButton.classList.add("active");
    }

    renderTimer();
  };

  const renderTimer = () => {
    const minutes = Math.floor(remainingSeconds / 60).toString().padStart(2, "0");
    const seconds = (remainingSeconds % 60).toString().padStart(2, "0");
    timerDisplay.textContent = `${minutes}:${seconds}`;
    if (timerRing) {
      const progress = totalSeconds === 0 ? 0 : ((totalSeconds - remainingSeconds) / totalSeconds) * 360;
      timerRing.style.setProperty("--progress", `${progress}deg`);
    }
  };

  const stopTimer = () => {
    window.clearInterval(timerId);
    timerId = null;
    startButton.textContent = "Başlat";
    if (timerStatus) {
      timerStatus.textContent = "Hazır";
    }
  };

  const playBeep = () => {
    if (!soundToggle || !soundToggle.checked) {
      return;
    }

    try {
      const context = new (window.AudioContext || window.webkitAudioContext)();
      const oscillator = context.createOscillator();
      const gain = context.createGain();

      oscillator.type = "sine";
      oscillator.frequency.value = 640;
      gain.gain.value = 0.08;

      oscillator.connect(gain);
      gain.connect(context.destination);

      oscillator.start();
      oscillator.stop(context.currentTime + 0.25);
    } catch {
      // Ignore audio errors silently.
    }
  };

  const setupGoal = () => {
    if (!dailyGoalInput || !dailyGoalProgress || !dailyGoalLabel || !focusPanel) {
      return;
    }

    const storedGoal = Number(localStorage.getItem("dailyFocusGoal")) || 90;
    dailyGoalInput.value = storedGoal;

    const updateGoalUi = () => {
      const goal = Math.max(Number(dailyGoalInput.value) || storedGoal, 15);
      const todayMinutes = Number(focusPanel.dataset.todayMinutes) || 0;
      const percent = Math.min(Math.round((todayMinutes / goal) * 100), 100);
      dailyGoalProgress.style.setProperty("--value", `${percent}%`);
      dailyGoalLabel.textContent = `${todayMinutes} / ${goal} dk (%${percent})`;
      localStorage.setItem("dailyFocusGoal", goal.toString());
    };

    dailyGoalInput.addEventListener("change", updateGoalUi);
    dailyGoalInput.addEventListener("input", updateGoalUi);
    updateGoalUi();
  };

  const setupToggles = () => {
    if (soundToggle) {
      const soundState = localStorage.getItem("focusSoundEnabled");
      soundToggle.checked = soundState ? soundState === "true" : true;
      soundToggle.addEventListener("change", () => {
        localStorage.setItem("focusSoundEnabled", soundToggle.checked.toString());
      });
    }

    if (notifyToggle) {
      const notifyState = localStorage.getItem("focusNotifyEnabled");
      notifyToggle.checked = notifyState ? notifyState === "true" : true;
      notifyToggle.addEventListener("change", () => {
        localStorage.setItem("focusNotifyEnabled", notifyToggle.checked.toString());
      });
    }
  };

  durationButtons.forEach((button) => {
    button.addEventListener("click", () => {
      if (timerId) {
        stopTimer();
      }

      setDuration(Number(button.dataset.minutes), true);
    });
  });

  if (applyCustomDurationButton && customMinutesInput) {
    applyCustomDurationButton.addEventListener("click", () => {
      if (timerId) {
        stopTimer();
      }

      setDuration(customMinutesInput.value);
    });

    customMinutesInput.addEventListener("keydown", (event) => {
      if (event.key === "Enter") {
        event.preventDefault();
        applyCustomDurationButton.click();
      }
    });
  }

  startButton.addEventListener("click", async () => {
    if (timerId) {
      stopTimer();
      return;
    }

    if (timerStatus) {
      timerStatus.textContent = "Odakta";
    }

    if (notifyToggle?.checked && "Notification" in window && Notification.permission === "default") {
      await Notification.requestPermission();
    }

    startButton.textContent = "Durdur";
    timerId = window.setInterval(() => {
      remainingSeconds -= 1;
      renderTimer();

      if (remainingSeconds <= 0) {
        stopTimer();
        playBeep();
        if (notifyToggle?.checked && "Notification" in window && Notification.permission === "granted") {
          new Notification("Pomodoro tamamlandı", { body: `${selectedMinutes} dakikalık odak seansı kaydediliyor.` });
        }
        completionForm.submit();
      }
    }, 1000);
  });

  resetButton.addEventListener("click", () => {
    if (timerId) {
      stopTimer();
    }

    remainingSeconds = totalSeconds;
    renderTimer();
  });

  setupGoal();
  setupToggles();
  renderTimer();
});
