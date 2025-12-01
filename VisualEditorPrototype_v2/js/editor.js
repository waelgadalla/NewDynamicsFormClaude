document.addEventListener('DOMContentLoaded', () => {
    
    // State
    let selectedFieldId = 'field_2'; // Default selection
    
    // Elements
    const propTabs = document.querySelectorAll('.prop-tab');
    const propPanels = {
        general: document.getElementById('panel-general'),
        validation: document.getElementById('panel-validation'),
        logic: document.getElementById('panel-logic'),
        data: document.getElementById('panel-data')
    };
    
    const canvasFields = document.querySelectorAll('.field-wrapper');
    const treeNodes = document.querySelectorAll('.tree-node');

    // --- Tab Switching (Inspector) ---
    propTabs.forEach(tab => {
        tab.addEventListener('click', () => {
            // UI Update
            propTabs.forEach(t => t.classList.remove('active'));
            tab.classList.add('active');
            
            // Panel Visibility
            const target = tab.dataset.target;
            Object.values(propPanels).forEach(p => p.classList.add('hidden')); // You'd need a hidden class utility
            if(propPanels[target]) {
                // Simplified for demo: just hiding/showing sections
                // In real implementation, toggling 'display: none' via CSS classes
                document.querySelectorAll('.prop-section').forEach(s => s.style.display = 'none');
                document.getElementById(`panel-${target}`).style.display = 'block';
            }
        });
    });

    // --- Selection Logic ---
    canvasFields.forEach(field => {
        field.addEventListener('click', (e) => {
            e.stopPropagation(); // Prevent clicking parent section
            selectField(field.id);
        });
    });
    
    treeNodes.forEach(node => {
        node.addEventListener('click', () => {
            const id = node.dataset.id;
            if(id) selectField(id);
        });
    });

    function selectField(id) {
        // 1. Visual Update Canvas
        canvasFields.forEach(f => f.classList.remove('selected'));
        const targetField = document.getElementById(id);
        if(targetField) targetField.classList.add('selected');
        
        // 2. Visual Update Tree
        treeNodes.forEach(n => n.classList.remove('active'));
        const targetNode = document.querySelector(`.tree-node[data-id="${id}"]`);
        if(targetNode) targetNode.classList.add('active');
        
        // 3. Update Inspector Content (Mock)
        const type = targetField.dataset.type;
        const label = targetField.querySelector('.form-label').textContent.replace('*', '').trim();
        
        document.getElementById('inspector-title').textContent = type;
        document.getElementById('inspector-id').textContent = id;
        
        // Update inputs
        const labelInput = document.querySelector('input[name="label_en"]');
        if(labelInput) labelInput.value = label;
    }

    // --- Toggle Switch Logic ---
    document.querySelectorAll('.toggle-switch').forEach(toggle => {
        toggle.addEventListener('click', () => {
            toggle.classList.toggle('active');
            // In real app, update state
        });
    });

    // --- Initialize ---
    // Hide non-active panels
    document.getElementById('panel-validation').style.display = 'none';
    document.getElementById('panel-logic').style.display = 'none';
    document.getElementById('panel-data').style.display = 'none';
    
    // Initial Selection
    selectField(selectedFieldId);
});
