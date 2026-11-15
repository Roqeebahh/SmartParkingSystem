const API_BASE = 'http://192.168.197.170:5062/api'; 
//const API_BASE = 'https://localhost:7240/api';

const App = (() => {
  'use strict';

  let allFaculties  = [];
  let currentView   = 'grid';
  let selectedFaculty = null;
  let modalFaculty  = null;

  const $ = id => document.getElementById(id);


  async function init() {
    await loadFacultiesAndSpaces();
    await loadStats();
  }

  async function loadFacultiesAndSpaces() {
    try {
      const res = await fetch(`${API_BASE}/parking/all`);
      if (!res.ok) throw new Error(`HTTP ${res.status}`);
      allFaculties = await res.json();
      renderGrid();
      renderFacultyNavList();
    } catch (err) {
      console.error('Failed to load faculty data:', err);
      $('facultiesGrid').innerHTML = `
        <div class="loading-state">
          <p style="color:var(--color-occupied)">⚠ Could not connect to the API.<br>
          Ensure the backend is running on <code>http://localhost:5000</code></p>
        </div>`;
      showToast('API connection failed. Is the backend running?', 'error');
    }
  }

  async function loadStats() {
    try {
      const res = await fetch(`${API_BASE}/parking/stats`);
      if (!res.ok) return;
      const s = await res.json();
      $('statTotal').textContent    = s.totalSpaces;
      $('statAvail').textContent    = s.availableSpaces;
      $('statOccupied').textContent = s.occupiedSpaces;
      $('statInactive').textContent = s.inactiveSpaces;
    } catch (_) {}
  }

  function renderGrid() {
    const grid = $('facultiesGrid');
    if (!allFaculties.length) {
      grid.innerHTML = '<div class="loading-state"><p>No faculty data available.</p></div>';
      return;
    }
    grid.innerHTML = allFaculties.map(f => buildFacultyCard(f)).join('');
  }

  function buildFacultyCard(f) {
    const monitored  = f.monitoringEnabled;
    const badgeCls   = monitored ? 'live' : 'soon';
    const badgeTxt   = monitored ? '● LIVE' : 'Coming Soon';
    const availCount = monitored ? f.spaces.filter(s => s.statusCode === 0).length : null;

    const spacesHtml = f.spaces.map(s => {
      const cls = monitored ? statusClass(s.statusCode) : 'inactive';
      return `<span class="space-chip ${cls}" id="chip-${s.spaceNumber}">${s.spaceNumber}</span>`;
    }).join('');

    const footerInfo = monitored
      ? `<span class="card-avail-count" style="color:var(--color-${availCount > 0 ? 'available' : 'occupied'})">
           ${availCount} / ${f.spaces.length} available
         </span>`
      : `<span class="card-avail-count" style="color:var(--color-inactive)">Monitoring not yet active</span>`;

    return `
      <article class="faculty-card ${monitored ? 'monitoring-enabled' : ''}"
               onclick="App.openModal(${f.facultyId})"
               role="button" tabindex="0"
               aria-label="View parking for ${f.faculty}"
               onkeydown="if(event.key==='Enter')App.openModal(${f.facultyId})">
        <div class="card-header">
          <h3 class="card-name">${f.faculty}</h3>
          <span class="card-badge ${badgeCls}">${badgeTxt}</span>
        </div>
        <div class="card-spaces">${spacesHtml}</div>
        <div class="card-footer">
          ${footerInfo}
          <span class="card-nav-hint">
            <svg viewBox="0 0 20 20" fill="currentColor">
              <path d="M10.894 2.553a1 1 0 00-1.788 0l-7 14a1 1 0 001.169 1.409l5-1.429A1 1 0 009 15.571V11a1 1 0 112 0v4.571a1 1 0 00.725.962l5 1.428a1 1 0 001.17-1.408l-7-14z"/>
            </svg>
            Navigate
          </span>
        </div>
      </article>`;
  }

  function statusClass(code) {
    return ['available', 'occupied', 'inactive'][code] ?? 'inactive';
  }

  function renderFacultyNavList(filter = '') {
    const list   = $('facultyNavList');
    const lower  = filter.toLowerCase();
    const filtered = filter
      ? allFaculties.filter(f => f.faculty.toLowerCase().includes(lower))
      : allFaculties;

    list.innerHTML = filtered.map(f => `
      <li class="nav-faculty-item ${selectedFaculty?.facultyId === f.facultyId ? 'active' : ''}"
          onclick="App.selectFaculty(${f.facultyId})"
          role="option"
          aria-selected="${selectedFaculty?.facultyId === f.facultyId}">
        <span class="nav-faculty-name">${f.faculty}</span>
        ${f.monitoringEnabled ? '<span class="nav-faculty-live">LIVE</span>' : ''}
      </li>`).join('');
  }

  function filterFacultyList(value) {
    renderFacultyNavList(value);
  }

  // ── Faculty Select ───────────────────────────────────────────────────────
  function selectFaculty(facultyId) {
    selectedFaculty = allFaculties.find(f => f.facultyId === facultyId);
    renderFacultyNavList($('facultySearch').value);
    if (selectedFaculty && window.Maps) {
      Maps.navigateTo(selectedFaculty);
    }
  }

  function openModal(facultyId) {
    modalFaculty = allFaculties.find(f => f.facultyId === facultyId);
    if (!modalFaculty) return;

    $('modalFacultyName').textContent = modalFaculty.faculty;

    fetch(`${API_BASE}/faculty/${facultyId}`)
      .then(r => r.json())
      .then(d => { $('modalFacultyDesc').textContent = d.description || ''; })
      .catch(() => { $('modalFacultyDesc').textContent = ''; });

    const monitored = modalFaculty.monitoringEnabled;
    $('modalComingSoon').style.display = monitored ? 'none' : 'flex';

    $('modalSpaces').innerHTML = modalFaculty.spaces.map(s => {
      const cls   = monitored ? statusClass(s.statusCode) : 'inactive';
      const label = monitored ? s.status : 'N/A';
      return `<div class="modal-space ${cls}" id="modal-chip-${s.spaceNumber}">
        ${s.spaceNumber}
        <div class="modal-space-label">${label}</div>
      </div>`;
    }).join('');

    $('modalBackdrop').style.display = 'flex';
    document.body.style.overflow = 'hidden';
  }

  function closeModal(e) {
    if (e && e.target !== $('modalBackdrop') && !e.target.closest('.modal-close')) return;
    $('modalBackdrop').style.display = 'none';
    document.body.style.overflow = '';
    modalFaculty = null;
  }

  function navigateToFaculty() {
    if (!modalFaculty) return;
    $('modalBackdrop').style.display = 'none';
    document.body.style.overflow = '';
    setView('map');
    selectFaculty(modalFaculty.facultyId);
  }

  // Real-time update 
  function updateSpaceChip(update) {
    const facultyGroup = allFaculties.find(f => f.facultyId === update.facultyId);
    if (!facultyGroup) return;

    const space = facultyGroup.spaces.find(s => s.spaceNumber === update.spaceNumber);
    if (space) {
      space.status      = update.status;
      space.statusCode  = update.statusCode;
      space.lastUpdated = update.lastUpdated;
    }

    const chip = document.getElementById(`chip-${update.spaceNumber}`);
    if (chip) {
      chip.className = `space-chip ${statusClass(update.statusCode)} updated`;
      setTimeout(() => chip.classList.remove('updated'), 700);
    }

    const modalChip = document.getElementById(`modal-chip-${update.spaceNumber}`);
    if (modalChip) {
      modalChip.className = `modal-space ${statusClass(update.statusCode)}`;
      modalChip.querySelector('.modal-space-label').textContent = update.status;
    }

    refreshCardFooter(facultyGroup);
    loadStats();
  }

  function refreshCardFooter(f) {
    const availCount = f.spaces.filter(s => s.statusCode === 0).length;
    const card = document.querySelector(`[onclick="App.openModal(${f.facultyId})"] .card-avail-count`);
    if (card) {
      card.style.color = `var(--color-${availCount > 0 ? 'available' : 'occupied'})`;
      card.textContent = `${availCount} / ${f.spaces.length} available`;
    }
  }

  //View Switching 
  function setView(view) {
    currentView = view;
    document.querySelectorAll('.view').forEach(v => v.classList.remove('active'));
    document.querySelectorAll('.toggle-btn').forEach(b => {
      b.classList.remove('active');
      b.setAttribute('aria-pressed', 'false');
    });
    document.getElementById(`${view}View`).classList.add('active');
    const btn = document.getElementById(`btn${view.charAt(0).toUpperCase() + view.slice(1)}`);
    btn.classList.add('active');
    btn.setAttribute('aria-pressed', 'true');
    if (view === 'map' && window.Maps) {
      Maps.ensureInitialised();
      requestAnimationFrame(() => Maps.ensureInitialised());
    }
  }


  function showToast(message, type = 'info', duration = 3500) {
    const container = $('toastContainer');
    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    toast.textContent = message;
    container.appendChild(toast);
    setTimeout(() => {
      toast.style.opacity   = '0';
      toast.style.transform = 'translateX(100%)';
      toast.style.transition = '250ms ease';
      setTimeout(() => toast.remove(), 260);
    }, duration);
  }

  return {
    init, setView, openModal, closeModal, navigateToFaculty,
    selectFaculty, filterFacultyList, updateSpaceChip, showToast,
    get faculties() { return allFaculties; }
  };
})();

window.App = App;
document.addEventListener('DOMContentLoaded', () => App.init());
