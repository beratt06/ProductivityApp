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

  if (!timerDisplay || !startButton || !resetButton || !durationInput || !completionForm) {
    return;
  }

  let selectedMinutes = Number(durationInput.value);
  let remainingSeconds = selectedMinutes * 60;
  let timerId = null;

  const setDuration = (minutes, keepActiveButton = false) => {
    selectedMinutes = Math.min(Math.max(Number(minutes) || 25, 1), 180);
    durationInput.value = selectedMinutes;
    remainingSeconds = selectedMinutes * 60;

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
  };

  const stopTimer = () => {
    window.clearInterval(timerId);
    timerId = null;
    startButton.textContent = "Başlat";
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

    if ("Notification" in window && Notification.permission === "default") {
      await Notification.requestPermission();
    }

    startButton.textContent = "Durdur";
    timerId = window.setInterval(() => {
      remainingSeconds -= 1;
      renderTimer();

      if (remainingSeconds <= 0) {
        stopTimer();
        if ("Notification" in window && Notification.permission === "granted") {
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

    remainingSeconds = selectedMinutes * 60;
    renderTimer();
  });
});
