document.addEventListener('DOMContentLoaded', () => {
    
    // --- Global State Mock ---
    const state = {
        currentTab: 'editor', // editor, preview, validation
        selectedFieldId: null,
        showLibrary: true,
        history: [], // For Undo
        historyIndex: -1
    };

    // --- DOM Elements ---
    const tabBtns = document.querySelectorAll('.tab-btn');
    const contentAreas = {
        editor: document.getElementById('tab-content-editor'),
        preview: document.getElementById('tab-content-preview'),
        validation: document.getElementById('tab-content-validation')
    };
    const formCanvas = document.getElementById('form-canvas');
    const propertiesPanel = document.getElementById('properties-panel');
    const libraryPanel = document.getElementById('library-sidebar');
    const btnToggleLibrary = document.getElementById('btn-toggle-library');

    // --- Tab Switching Logic ---
    tabBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            const tabName = btn.dataset.tab;
            switchTab(tabName);
        });
    });

    function switchTab(tabName) {
        // Update Buttons
        tabBtns.forEach(b => b.classList.toggle('active', b.dataset.tab === tabName));
        
        // Reset views
        formCanvas.classList.remove('preview-mode', 'hidden');
        document.getElementById('json-view').classList.add('hidden');
        document.getElementById('validation-summary').classList.remove('show');

        // Handle Specific Modes
        if(tabName === 'preview') {
            formCanvas.classList.add('preview-mode');
        } else if (tabName === 'json') {
            formCanvas.classList.add('hidden'); // Hide designer canvas
            document.getElementById('json-view').classList.remove('hidden');
        } else if(tabName === 'validation') {
            document.getElementById('validation-summary').classList.add('show');
        }
        
        state.currentTab = tabName;
    }

    // --- Sidebar Toggle ---
    if(btnToggleLibrary) {
        btnToggleLibrary.addEventListener('click', () => {
            libraryPanel.classList.toggle('collapsed');
        });
    }

    // --- Mock Field Selection ---
    // Using event delegation for static + dynamic elements
    formCanvas.addEventListener('click', (e) => {
        if (state.currentTab === 'preview') return;

        // Check if clicking a field
        const fieldNode = e.target.closest('.field-node');
        
        if (fieldNode) {
            // FIELD SELECTION
            e.stopPropagation(); // Stop bubbling to background
            document.querySelectorAll('.field-node').forEach(n => n.classList.remove('selected'));
            fieldNode.classList.add('selected');
            state.selectedFieldId = fieldNode.id;
            renderPropertiesPanel('field', { type: fieldNode.dataset.type, label: fieldNode.dataset.label });
        } else {
            // BACKGROUND CLICK -> MODULE PROPERTIES
            document.querySelectorAll('.field-node').forEach(n => n.classList.remove('selected'));
            state.selectedFieldId = null;
            renderPropertiesPanel('module');
        }
    });

    // Handle Workflow Step Settings Click
    const stepSettingsBtn = document.querySelector('#wf-step-start button');
    if(stepSettingsBtn) {
        stepSettingsBtn.addEventListener('click', (e) => {
            e.stopPropagation(); // Prevent triggering step navigation
            renderPropertiesPanel('workflow');
        });
    }

    // --- Dynamic Properties Panel Renderer ---
    function renderPropertiesPanel(mode, data = {}) {
        const panelContainer = document.getElementById('properties-panel');
        
        let html = '';

        if (mode === 'field') {
            html = `
                <div class="p-4 border-b bg-white sticky top-0">
                    <h3 class="font-bold text-sm">${data.type} Properties</h3>
                </div>
                <div class="prop-group">
                    <h4 style="font-weight:700; margin-bottom:1rem; font-size:0.8rem; text-transform:uppercase; color:#94a3b8; cursor:pointer; display:flex; align-items:center; justify-content:space-between;">
                        General <i class="fa-solid fa-chevron-down"></i>
                    </h4>
                    <div class="prop-content">
                        <div class="mb-4">
                            <label class="prop-label">Label (EN)</label>
                            <input type="text" class="prop-input" value="${data.label}">
                        </div>
                        <div class="mb-4">
                            <label class="prop-label">Label (FR)</label>
                            <input type="text" class="prop-input" value="Label FR">
                        </div>
                    </div>
                </div>
                <div class="prop-group">
                    <h4 style="font-weight:700; margin-bottom:1rem; font-size:0.8rem; text-transform:uppercase; color:#94a3b8; cursor:pointer; display:flex; align-items:center; justify-content:space-between;">
                        Validation <i class="fa-solid fa-chevron-down"></i>
                    </h4>
                    <div class="prop-content">
                         <div class="flex items-center gap-2 mb-2">
                            <input type="checkbox" id="chk-req">
                            <label for="chk-req" style="font-size:0.875rem;">Required</label>
                        </div>
                    </div>
                </div>
                <div class="prop-group">
                    <h4 style="font-weight:700; margin-bottom:1rem; font-size:0.8rem; text-transform:uppercase; color:#94a3b8; cursor:pointer; display:flex; align-items:center; justify-content:space-between;">
                        Logic & Rules <i class="fa-solid fa-chevron-down"></i>
                    </h4>
                    <div class="prop-content">
                        <div class="mb-2">
                            <button class="btn btn-primary w-full" style="width:100%; font-size:0.8rem;"><i class="fa-solid fa-plus"></i> Add Rule</button>
                        </div>
                        <div style="background:#f1f5f9; padding:0.5rem; border-radius:4px; font-size:0.75rem; border:1px solid #e2e8f0;">
                            <div style="display:flex; justify-content:space-between; margin-bottom:4px;">
                                <span style="font-weight:bold;">Visibility Rule</span>
                                <i class="fa-solid fa-trash text-danger" style="cursor:pointer;"></i>
                            </div>
                            <div style="color:#64748b;">IF [OrgType] == 'NonProfit' THEN Show</div>
                        </div>
                    </div>
                </div>
            `;
        } else if (mode === 'module') {
            html = `
                <div class="p-4 border-b bg-white sticky top-0" style="background:#f8fafc;">
                    <h3 class="font-bold text-sm"><i class="fa-solid fa-box-open"></i> Module Properties</h3>
                </div>
                <div class="prop-group">
                    <h4 style="font-weight:700; margin-bottom:1rem; font-size:0.8rem; text-transform:uppercase; color:#94a3b8; cursor:pointer; display:flex; align-items:center; justify-content:space-between;">
                        Meta Data <i class="fa-solid fa-chevron-down"></i>
                    </h4>
                    <div class="prop-content">
                        <div class="mb-4">
                            <label class="prop-label">Module Title (EN)</label>
                            <input type="text" class="prop-input" value="Applicant Info">
                        </div>
                        <div class="mb-4">
                            <label class="prop-label">Module Title (FR)</label>
                            <input type="text" class="prop-input" value="Infos du demandeur">
                        </div>
                         <div class="mb-4">
                            <label class="prop-label">Description</label>
                            <textarea class="prop-input" rows="3">Collects primary contact information.</textarea>
                        </div>
                    </div>
                </div>
                <div class="prop-group">
                    <h4 style="font-weight:700; margin-bottom:1rem; font-size:0.8rem; text-transform:uppercase; color:#94a3b8; cursor:pointer; display:flex; align-items:center; justify-content:space-between;">
                        Database <i class="fa-solid fa-chevron-down"></i>
                    </h4>
                    <div class="prop-content">
                        <div class="mb-4">
                            <label class="prop-label">Table Name</label>
                            <input type="text" class="prop-input" value="tbl_App_Info">
                        </div>
                    </div>
                </div>
            `;
        } else if (mode === 'workflow') {
            html = `
                <div class="p-4 border-b bg-white sticky top-0" style="background:#eff6ff;">
                    <h3 class="font-bold text-sm" style="color:#1e40af;"><i class="fa-solid fa-diagram-project"></i> Workflow Step</h3>
                </div>
                <div class="prop-group">
                    <h4 style="font-weight:700; margin-bottom:1rem; font-size:0.8rem; text-transform:uppercase; color:#94a3b8; cursor:pointer; display:flex; align-items:center; justify-content:space-between;">
                        Step Config <i class="fa-solid fa-chevron-down"></i>
                    </h4>
                    <div class="prop-content">
                        <div class="mb-4">
                            <label class="prop-label">Step Name</label>
                            <input type="text" class="prop-input" value="Start">
                        </div>
                         <div class="mb-4">
                            <label class="prop-label">Assigned Role</label>
                            <select class="prop-input">
                                <option>Public User</option>
                                <option>Internal Officer</option>
                                <option>System Admin</option>
                            </select>
                        </div>
                    </div>
                </div>
            `;
        }

        panelContainer.innerHTML = html;
    }

    // --- Drag/Drop Simulation (Click to Add) ---
    const libraryItems = document.querySelectorAll('.library-item');
    libraryItems.forEach(item => {
        item.addEventListener('click', () => {
            // Simulate adding a field
            const type = item.querySelector('span').textContent;
            addFieldToCanvas(type);
        });
    });

    function addFieldToCanvas(type) {
        const id = Date.now();
        const html = `
            <div class="field-node selected" data-type="${type}" data-label="New ${type}" id="field-${id}">
                <div class="field-header">
                    <div class="field-title">
                        <span class="field-type-badge">${type}</span>
                        <span class="field-label-display">New ${type}</span>
                    </div>
                    <div class="field-actions">
                        <button class="btn btn-icon" title="Move Up">↑</button>
                        <button class="btn btn-icon" title="Move Down">↓</button>
                        <button class="btn btn-icon text-danger" title="Delete" onclick="this.closest('.field-node').remove()">×</button>
                    </div>
                </div>
                <div class="field-body">
                    <label class="form-label">New ${type}</label>
                    <input type="text" class="form-input" placeholder="Enter value..." disabled>
                </div>
            </div>
        `;
        
        // Deselect existing
        document.querySelectorAll('.field-node').forEach(n => n.classList.remove('selected'));
        
        // Insert
        const tempDiv = document.createElement('div');
        tempDiv.innerHTML = html;
        const newNode = tempDiv.firstElementChild;
        
        // If a field is selected, insert after, else append
        const selected = formCanvas.querySelector('.field-node.selected');
        if(selected) {
            selected.after(newNode);
        } else {
            formCanvas.appendChild(newNode);
        }
        
        // Trigger Property update
        renderPropertiesPanel('field', { type: type, label: `New ${type}` });
        
        // Save to history (mock)
        pushHistory();
    }

    function pushHistory() {
        // In real app, serialize state
        console.log("Action recorded to Undo stack");
    }

    // Initialize with Module Properties (as if background was clicked)
    renderPropertiesPanel('module');

    // --- Workflow Navigation (Version A) ---
    const wfSteps = document.querySelectorAll('.wf-step');
    wfSteps.forEach(step => {
        step.addEventListener('click', () => {
            wfSteps.forEach(s => s.classList.remove('active'));
            step.classList.add('active');
            // Simulate loading different form fields
            simulateContentLoad();
        });
    });

    function simulateContentLoad() {
        formCanvas.style.opacity = '0.5';
        setTimeout(() => {
            formCanvas.style.opacity = '1';
        }, 200);
    }

    // --- Property Group Collapsing ---
    // Use event delegation on the properties panel
    if (propertiesPanel) {
        propertiesPanel.addEventListener('click', (e) => {
            // Check if a header (h4) or its icon was clicked
            const header = e.target.closest('h4');
            if (header && header.parentElement.classList.contains('prop-group')) {
                const group = header.parentElement;
                
                // Toggle a 'collapsed' class on the group
                group.classList.toggle('collapsed');
                
                // Rotate the icon if present
                const icon = header.querySelector('i');
                if (icon) {
                    // Simple rotation logic via CSS transform
                    if (group.classList.contains('collapsed')) {
                        icon.style.transform = 'rotate(-90deg)';
                    } else {
                        icon.style.transform = 'rotate(0deg)';
                    }
                }
            }
        });
    }

    // --- Resizable Sidebars Logic ---
    const makeResizable = (resizer, target, direction) => {
        if(!resizer || !target) return;

        let startX = 0;
        let startWidth = 0;

        const onMouseMove = (e) => {
            const dx = e.clientX - startX;
            if (direction === 'right') {
                // Left sidebar: moving right increases width
                target.style.width = `${startWidth + dx}px`;
            } else {
                // Right sidebar: moving right decreases width (as it expands to left)
                target.style.width = `${startWidth - dx}px`;
            }
            // Prevent sidebar transition animation during drag for smoothness
            target.style.transition = 'none';
        };

        const onMouseUp = () => {
            document.removeEventListener('mousemove', onMouseMove);
            document.removeEventListener('mouseup', onMouseUp);
            resizer.classList.remove('resizing');
            document.body.style.cursor = 'default';
            // Restore transition
            target.style.transition = 'width 0.1s ease-out';
        };

        resizer.addEventListener('mousedown', (e) => {
            startX = e.clientX;
            startWidth = parseInt(window.getComputedStyle(target).width, 10);
            
            document.addEventListener('mousemove', onMouseMove);
            document.addEventListener('mouseup', onMouseUp);
            
            resizer.classList.add('resizing');
            document.body.style.cursor = 'col-resize';
            e.preventDefault(); // Prevent text selection
        });
    };

    // Initialize Resizers
    makeResizable(document.getElementById('resizer-left'), document.getElementById('library-sidebar'), 'right');
    makeResizable(document.getElementById('resizer-right'), document.getElementById('properties-panel'), 'left');

});