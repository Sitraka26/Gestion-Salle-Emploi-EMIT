// Site-wide JS: SignalR notifications and simple UX helpers
(function () {
	"use strict";

	// Create a Bootstrap toast and show it
	function showToast(message) {
		const container = document.getElementById('toastContainer');
		if (!container) return;

		const toastEl = document.createElement('div');
		toastEl.className = 'toast align-items-center text-bg-primary border-0 mb-2';
		toastEl.setAttribute('role', 'status');
		toastEl.setAttribute('aria-live', 'polite');
		toastEl.setAttribute('aria-atomic', 'true');

		toastEl.innerHTML = `
			<div class="d-flex">
				<div class="toast-body">${message}</div>
				<button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
			</div>`;

		container.appendChild(toastEl);

		const toast = new bootstrap.Toast(toastEl, { delay: 5000 });
		toast.show();

		// Remove from DOM after hidden
		toastEl.addEventListener('hidden.bs.toast', () => toastEl.remove());
	}

	// SignalR connection
	function startSignalR() {
		if (typeof signalR === 'undefined' || !signalR.HubConnectionBuilder) return;

		const connection = new signalR.HubConnectionBuilder()
			.withUrl('/hubs/notifications')
			.withAutomaticReconnect()
			.build();

		connection.on('ReceiveNotification', function (message) {
			showToast(message);
		});

		connection.start().catch(function (err) {
			console.error('SignalR connection error:', err.toString());
		});
	}

	// Init on DOM ready
	document.addEventListener('DOMContentLoaded', function () {
		startSignalR();
	});

})();
