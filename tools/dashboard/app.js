const state = { language: 'en', config: null, result: null, displayMode: 'raw' };

const text = {
  en: { eyebrow:'QUALITY INSPECTION', title:'Square Rod Measurement', settings:'Settings', dataDirectory:'Measurement data directory', measurementFile:'Measurement file', loading:'Loading files…', refresh:'Refresh', runMeasurement:'Run measurement', wholeRod:'WHOLE ROD', wholeRodResults:'Whole-rod results', awaitingResult:'Awaiting result', rodLength:'Rod length', diagonal1:'Diagonal 1', diagonal2:'Diagonal 2', diagonalDifference:'Diagonal difference', headEndFace:'Head end-face perpendicularity', tailEndFace:'Tail end-face perpendicularity', crossSection:'CROSS-SECTION', cornerGeometry:'Corner geometry', meanValues:'Final values = CSV mean', diagramNote:'C1–C4 follow the camera/corner mapping. Diagonals use chamfer midpoints.', edgeDimensions:'EDGE DIMENSIONS', edgeAndSymmetry:'Edge and symmetry values', calibration:'CALIBRATION', calibrationCheck:'Calibration and compensation check', pathSettings:'Paths and measurement settings', resultDirectory:'Result save directory', scriptPath:'Measurement script path', calibrationPath:'Calibration model path', truthPath:'Manual-truth CSV path', sliceSpacing:'Slice spacing (mm)', pathNote:'Paths are saved locally on this computer.', cancel:'Cancel', save:'Save settings', corner:'Corner', perpendicularity:'Perpendicularity', chamferLength:'Chamfer length', projectionX:'Horizontal projection (X)', projectionZ:'Height projection (Z)', camera:'Camera', model:'Model', bias:'Corner bias', manualTruth:'Manual truth check', truth:'Manual truth', measured:'Measured', difference:'Difference', noCalibration:'Calibration details are not available.', endFacePending:'Requires end-face calculation from the measurement script.' },
  zh: { eyebrow:'质量检测', title:'方棒测量', settings:'设置', dataDirectory:'检测数据目录', measurementFile:'检测文件', loading:'正在读取文件…', refresh:'刷新', runMeasurement:'开始测量', wholeRod:'整棒数据', wholeRodResults:'整棒结果', awaitingResult:'等待测量结果', rodLength:'棒长', diagonal1:'对角线 1', diagonal2:'对角线 2', diagonalDifference:'对角线差', headEndFace:'头部端面垂直度', tailEndFace:'尾部端面垂直度', crossSection:'横截面', cornerGeometry:'四角几何数据', meanValues:'最终值 = CSV 平均值', diagramNote:'C1–C4 对应相机/角点编号；对角线按倒角中点计算。', edgeDimensions:'边长数据', edgeAndSymmetry:'边长和对称性', calibration:'标定', calibrationCheck:'标定与补偿核查', pathSettings:'路径和测量设置', resultDirectory:'结果保存目录', scriptPath:'测量脚本路径', calibrationPath:'标定模型路径', truthPath:'人工真值 CSV 路径', sliceSpacing:'切片间距（mm）', pathNote:'路径会保存于本机。', cancel:'取消', save:'保存设置', corner:'角', perpendicularity:'垂直度', chamferLength:'倒角长度', projectionX:'横向投影（X）', projectionZ:'高度投影（Z）', camera:'相机', model:'模型', bias:'角点偏置', manualTruth:'人工真值核查', truth:'人工真值', measured:'当前测量', difference:'偏差', noCalibration:'当前无法读取标定信息。', endFacePending:'需要由测量脚本提供端面垂直度计算。' }
};
Object.assign(text.en, {
  measurementVisuals: 'MEASUREMENT VISUALS', visualGuide: 'Measurement visual guide',
  crossSectionMap: 'Edge and diagonal map', crossSectionTag: 'Top view',
  crossSectionNote: 'A–D are free four-corner edge lengths. D1/D2 use chamfer midpoints M1–M4.',
  endFaceVisual: 'END-FACE VIEW', endFaceMap: 'Head and tail perpendicularity', sideView: 'Side view',
  head: 'Head', tail: 'Tail', rodAxis: 'Rod axis',
  endfaceNote: 'Values will appear here when the measurement script provides end-face results.',
  chamferMap: 'Chamfer, perpendicularity and projections', cornerDetail: 'Enlarged corner',
  cornerData: 'Four-corner data',
  chamferNote: 'The chamfer length is T1–T2. Projection X and Projection Z are the horizontal and height components of the same segment.'
});
Object.assign(text.zh, {
  measurementVisuals: '测量示意图', visualGuide: '测量数据可视化说明',
  crossSectionMap: '边长与对角线示意图', crossSectionTag: '横截面视图',
  crossSectionNote: 'A–D 为自由四角测量的边长；D1/D2 按倒角中点 M1–M4 计算。',
  endFaceVisual: '端面视图', endFaceMap: '头尾端面垂直度', sideView: '纵向侧视图',
  head: '头部', tail: '尾部', rodAxis: '棒轴',
  endfaceNote: '测量脚本提供端面测量结果后，数值会显示在这里。',
  chamferMap: '倒角、垂直度与投影', cornerDetail: '放大角部',
  cornerData: '四角数据',
  chamferNote: '倒角长度为 T1–T2；投影 X 和投影 Z 是同一倒角段的横向与高度分量。'
});
Object.assign(text.en, {
  headFaceAngles: 'Head: face-to-end angles',
  tailFaceAngles: 'Tail: face-to-end angles'
});
Object.assign(text.zh, {
  headFaceAngles: '\u5934\u90e8\uff1a\u56db\u9762\u4e0e\u7aef\u9762\u5939\u89d2',
  tailFaceAngles: '\u5c3e\u90e8\uff1a\u56db\u9762\u4e0e\u7aef\u9762\u5939\u89d2'
});
const t = key => text[state.language][key] || key;
const $ = selector => document.querySelector(selector);
const currentSummary = () => state.result?.[state.displayMode === 'corrected' ? 'corrected_summary' : 'raw_summary'] || state.result?.summary || {};
const format = value => value === undefined || value === null || value === '' || Number.isNaN(Number(value)) ? '—' : Number(value).toFixed(3);

async function api(url, options) {
  const response = await fetch(url, options);
  const data = await response.json();
  if (!response.ok) throw new Error(data.error || 'Request failed');
  return data;
}
function notice(message = '', kind = '') { const el = $('#notice'); el.textContent = message; el.className = `notice ${kind}`; }
function renderLanguage() {
  document.documentElement.lang = state.language === 'en' ? 'en' : 'zh-CN';
  document.querySelectorAll('[data-i18n]').forEach(el => { el.textContent = t(el.dataset.i18n); });
  $('#languageButton').textContent = state.language === 'en' ? '中文' : 'EN';
  renderCorners(); renderResult(); renderCalibrationV2(); updateAutoButton();
}
function renderCorners() {
  const mapping = [{obj:3,point:'P3',edge:'A / D'}, {obj:1,point:'P1',edge:'A / B'}, {obj:2,point:'P2',edge:'C / D'}, {obj:4,point:'P4',edge:'B / C'}];
  const summary = currentSummary();
  $('#cornerGrid').innerHTML = mapping.map((item, index) => `<article class="corner-card"><div class="corner-title"><strong>${t('corner')} ${index + 1}</strong><span>${item.point} · ${t('camera')} ${item.obj}</span></div><ul class="corner-list"><li><span>${t('perpendicularity')}</span><strong>${format(summary[`obj${item.obj}_verticality_error_deg`])} °</strong></li><li><span>${t('chamferLength')}</span><strong>${format(summary[`obj${item.obj}_chamfer_mm`])} mm</strong></li><li><span>${t('projectionX')}</span><strong>${format(summary[`obj${item.obj}_projection_x_mm`])} mm</strong></li><li><span>${t('projectionZ')}</span><strong>${format(summary[`obj${item.obj}_projection_z_mm`])} mm</strong></li></ul></article>`).join('');
}
function derived(summary, key) {
  const values = { diagonalDifference:['diag1_M1_M2_mm','diag2_M3_M4_mm'], aMinusC:['A_mm','C_mm'], bMinusD:['B_mm','D_mm'] }[key];
  if (!values || values.some(name => summary[name] === undefined || summary[name] === '')) return null;
  return Number(summary[values[0]]) - Number(summary[values[1]]);
}
function renderResult() {
  const summary = currentSummary();
  document.querySelectorAll('[data-field]').forEach(el => { el.textContent = format(summary[el.dataset.field]); });
  document.querySelectorAll('[data-derived]').forEach(el => { el.textContent = format(derived(summary, el.dataset.derived)); });
  document.querySelectorAll('.pending').forEach(card => card.title = t('endFacePending'));
  $('#resultSource').textContent = state.result ? `${state.result.slice_count} ${state.language === 'en' ? 'slices · CSV mean' : '个切片 · CSV 平均值'}` : t('awaitingResult');
}
function renderCalibration() {
  const cal = state.calibration;
  const container = $('#calibrationContent');
  if (!cal?.available) { container.innerHTML = `<p class="empty-state">${cal?.message || t('noCalibration')}</p>`; return; }
  $('#modelVersion').textContent = `${t('calibration')} v${cal.version}`;
  const biases = Object.entries(cal.corner_biases || {}).map(([point, value]) => `<tr><td>${point}</td><td>${format(value[0])}</td><td>${format(value[1])}</td></tr>`).join('') || '<tr><td colspan="3">—</td></tr>';
  const truth = state.result?.truth_check;
  const truthTable = truth?.available ? `<table><thead><tr><th>${t('manualTruth')}</th><th>${t('truth')}</th><th>${t('measured')}</th><th>${t('difference')}</th></tr></thead><tbody>${truth.comparisons.map(row => `<tr><td>${row.name}</td><td>${format(row.truth)}</td><td>${format(row.measured)}</td><td>${format(row.difference)}</td></tr>`).join('')}</tbody></table>` : `<p class="empty-state">${truth?.message || (state.language === 'en' ? 'Run a measurement after configuring a manual-truth CSV to compare values.' : '配置人工真值 CSV 后运行一次测量，即可进行数值对比。')}</p>`;
  container.innerHTML = `<div class="calibration-overview"><div class="calibration-item"><span>${t('model')}</span><strong>${cal.model}</strong></div><div class="calibration-item"><span>${t('calibration')}</span><strong>v${cal.version}</strong></div><div class="calibration-item"><span>Path</span><strong>${cal.path}</strong></div></div><div class="calibration-grid"><div><p class="eyebrow">${t('bias')}</p><div class="table-wrap"><table><thead><tr><th>Point</th><th>ΔX (mm)</th><th>ΔZ (mm)</th></tr></thead><tbody>${biases}</tbody></table></div></div><div><p class="eyebrow">${t('manualTruth')}</p><div class="table-wrap">${truthTable}</div></div></div>`;
}
function renderCalibrationV2() {
  const cal = state.calibration;
  const container = $('#calibrationContent');
  const cn = state.language === 'zh';
  if (!cal?.available) { container.innerHTML = `<p class="empty-state">${cal?.message || 'Calibration details are not available.'}</p>`; return; }
  $('#modelVersion').textContent = `${cn ? '\u6807\u5b9a' : 'Calibration'} v${cal.version}`;
  const standardRows = Object.entries(cal.standard || {}).map(([name, value]) => `<tr><td>${name}</td><td>${format(value.A)}</td><td>${format(value.B)}</td><td>${format(value.C)}</td><td>${format(value.D)}</td></tr>`).join('') || '<tr><td colspan="5">—</td></tr>';
  const biasRows = Object.entries(cal.corner_biases || {}).map(([point, value]) => `<tr><td>${point}</td><td>${format(value[0])}</td><td>${format(value[1])}</td></tr>`).join('') || '<tr><td colspan="3">—</td></tr>';
  const offsets = state.config?.edge_offsets_mm || cal.manual_edge_offsets_mm || {};
  const offsetInputs = ['A','B','C','D'].map(edge => `<label>${edge}<input name="offset_${edge}" type="number" step="any" value="${Number(offsets[edge] || 0)}"></label>`).join('');
  const endfaceOffsets = state.config?.endface_angle_offsets_deg || cal.manual_endface_angle_offsets_deg || {head:{},tail:{}};
  const endfaceInputs = ['head','tail'].map(end => `<div class="endface-offset-block"><strong>${end === 'head' ? (cn ? '\u5934\u90e8' : 'Head') : (cn ? '\u5c3e\u90e8' : 'Tail')}</strong><div class="endface-offset-inputs">${['A','B','C','D'].map(face => `<label>${face}<input name="endface_${end}_${face}" type="number" step="any" value="${Number(endfaceOffsets[end]?.[face] || 0)}"></label>`).join('')}</div></div>`).join('');
  const mode = state.displayMode;
  container.innerHTML = `<div class="calibration-overview"><div class="calibration-item"><span>${cn ? '\u6a21\u578b' : 'Model'}</span><strong>${cal.model}</strong></div><div class="calibration-item"><span>${cn ? '\u6807\u5b9a\u7248\u672c' : 'Calibration version'}</span><strong>v${cal.version}</strong></div><div class="calibration-item"><span>${cn ? '\u6a21\u578b\u8def\u5f84' : 'Model path'}</span><strong>${cal.path}</strong></div></div><div class="calibration-three-grid"><div><p class="eyebrow">${cn ? '\u6807\u5b9a\u771f\u503c' : 'CALIBRATION TRUTH'}</p><div class="table-wrap"><table><thead><tr><th>${cn ? '\u4f4d\u7f6e' : 'Position'}</th><th>A</th><th>B</th><th>C</th><th>D</th></tr></thead><tbody>${standardRows}</tbody></table></div></div><div><p class="eyebrow">${cn ? '\u89d2\u70b9\u6807\u5b9a\u504f\u7f6e' : 'CORNER BIAS'}</p><div class="table-wrap"><table><thead><tr><th>Point</th><th>ΔX mm</th><th>ΔZ mm</th></tr></thead><tbody>${biasRows}</tbody></table></div></div><div class="manual-compensation"><p class="eyebrow">${cn ? '\u4eba\u5de5\u8865\u507f' : 'MANUAL COMPENSATION'}</p><div class="mode-toggle"><button type="button" data-summary-mode="raw" class="${mode === 'raw' ? 'active' : ''}">${cn ? '\u539f\u59cb\u503c' : 'Raw'}</button><button type="button" data-summary-mode="corrected" class="${mode === 'corrected' ? 'active' : ''}">${cn ? '\u8865\u507f\u540e' : 'Corrected'}</button></div><form id="manualCompensationForm"><p class="offset-section-title">${cn ? '\u8fb9\u957f\u8865\u507f (mm)' : 'EDGE OFFSETS (mm)'}</p><div class="offset-inputs">${offsetInputs}</div><p class="offset-section-title">${cn ? '\u7aef\u9762\u5939\u89d2\u8865\u507f (\u00b0)' : 'END-FACE ANGLE OFFSETS (\u00b0)'}</p>${endfaceInputs}<button class="primary-button" type="submit">${cn ? '\u4fdd\u5b58\u8865\u507f' : 'Save compensation'}</button></form><p>${cn ? '\u8fb9\u957f\u548c\u516b\u4e2a\u7aef\u9762\u5939\u89d2\u8865\u507f\u53ea\u4fee\u6b63\u663e\u793a\u503c\uff0c\u4e0d\u8986\u76d6\u539f\u59cb CSV \u6216\u6807\u5b9a\u6a21\u578b\u3002' : 'Edge and eight face-to-end angle offsets correct displayed values only; raw CSV and calibration models are preserved.'}</p></div></div>`;
  container.querySelectorAll('[data-summary-mode]').forEach(button => button.addEventListener('click', () => { state.displayMode = button.dataset.summaryMode; renderLanguage(); }));
  $('#manualCompensationForm').addEventListener('submit', async event => {
    event.preventDefault();
    const form = event.currentTarget;
    const edge_offsets_mm = {};
    const endface_angle_offsets_deg = {head:{}, tail:{}};
    ['A','B','C','D'].forEach(edge => edge_offsets_mm[edge] = Number(form.elements[`offset_${edge}`].value || 0));
    ['head','tail'].forEach(end => ['A','B','C','D'].forEach(face => {
      endface_angle_offsets_deg[end][face] = Number(form.elements[`endface_${end}_${face}`].value || 0);
    }));
    try {
      state.config = await api('/api/config', {method:'POST', headers:{'Content-Type':'application/json'}, body:JSON.stringify({edge_offsets_mm, endface_angle_offsets_deg})});
      if (state.result?.raw_summary) {
        const raw = state.result.raw_summary;
        const corrected = {...raw};
        ['A','B','C','D'].forEach(edge => {
          const key = `${edge}_mm`;
          if (raw[key] !== '' && raw[key] !== undefined) corrected[key] = (Number(raw[key]) + edge_offsets_mm[edge]).toFixed(6);
        });
        ['head','tail'].forEach(end => {
          const angles = [];
          ['A','B','C','D'].forEach(face => {
            const key = `${end}_${face}_endface_angle_deg`;
            if (raw[key] === '' || raw[key] === undefined) return;
            const value = Number(raw[key]) + endface_angle_offsets_deg[end][face];
            corrected[key] = value.toFixed(6);
            angles.push(value);
          });
          if (angles.length === 4) corrected[`${end}_endface_verticality_deg`] = (angles.reduce((sum, value) => sum + Math.abs(90 - value), 0) / 4).toFixed(6);
        });
        state.result.corrected_summary = corrected;
      }
      state.displayMode = 'corrected';
      await loadCalibration();
      renderLanguage();
      notice(cn ? '\u8865\u507f\u5df2\u4fdd\u5b58\uff0c\u5f53\u524d\u9875\u9762\u5df2\u5207\u6362\u5230\u8865\u507f\u540e\u6570\u503c\u3002' : 'Compensation saved; corrected values are now displayed.', 'success');
    } catch (error) {
      notice(error.message, 'error');
    }
  });
}

function updateAutoButton() {
  const button = $('#autoDetectButton');
  if (!button || !state.config) return;
  const enabled = Boolean(state.config.auto_measure_enabled);
  button.textContent = enabled ? (state.language === 'zh' ? '停止自动检测' : 'Stop auto detection') : (state.language === 'zh' ? '启动自动检测' : 'Start auto detection');
  button.classList.toggle('auto-active', enabled);
}

async function loadInputs() {
  const data = await api('/api/inputs');
  const select = $('#fileSelect');
  select.innerHTML = data.files.length ? data.files.map(file => `<option value="${file.path}">[${file.kind.toUpperCase()}] ${file.relative} · ${file.modified.replace('T', ' ')}</option>`).join('') : `<option value="">${state.language === 'en' ? 'No HOBJ or TIFF sets found' : '未找到 HOBJ 或 TIFF 图组'}</option>`;
}
async function loadConfig() { state.config = await api('/api/config'); $('#dataRoot').textContent = state.config.data_root; const form = $('#settingsForm'); Object.keys(state.config).filter(key => form.elements[key]).forEach(key => form.elements[key].value = state.config[key]); updateAutoButton(); }
async function loadCalibration() { state.calibration = await api('/api/calibration'); renderCalibrationV2(); }
async function initialize() { try { await loadConfig(); await Promise.all([loadInputs(), loadCalibration()]); renderLanguage(); } catch (error) { notice(error.message, 'error'); } }
$('#refreshButton').addEventListener('click', async () => { notice(t('loading')); try { await loadInputs(); notice(state.language === 'en' ? 'File list refreshed.' : '文件列表已刷新。', 'success'); } catch (error) { notice(error.message, 'error'); } });
$('#autoDetectButton').addEventListener('click', async () => { const button = $('#autoDetectButton'); button.disabled = true; try { const enabled = !Boolean(state.config?.auto_measure_enabled); state.config = await api('/api/config', {method:'POST', headers:{'Content-Type':'application/json'}, body:JSON.stringify({auto_measure_enabled: enabled})}); updateAutoButton(); notice(enabled ? (state.language === 'zh' ? '自动检测已启动：将只处理新增或更新且文件稳定的 HOBJ。' : 'Automatic detection started. Only new or updated stable HOBJ files will be processed.') : (state.language === 'zh' ? '自动检测已停止。' : 'Automatic detection stopped.'), 'success'); } catch (error) { notice(error.message, 'error'); } finally { button.disabled = false; } });
$('#measureButton').addEventListener('click', async () => { const input_path = $('#fileSelect').value; if (!input_path) { notice(state.language === 'en' ? 'Select a measurement file first.' : '请先选择检测文件。', 'error'); return; } const button = $('#measureButton'); button.disabled = true; button.textContent = state.language === 'en' ? 'Measuring…' : '正在测量…'; notice(state.language === 'en' ? 'The measurement is running. Large HOBJ files may take a moment.' : '正在运行检测，大型 HOBJ 文件可能需要一些时间。'); try { state.result = await api('/api/measure', {method:'POST', headers:{'Content-Type':'application/json'}, body:JSON.stringify({input_path})}); renderLanguage(); notice(state.language === 'en' ? `Measurement complete. CSV saved to ${state.result.csv_path}` : `检测完成，CSV 已保存至 ${state.result.csv_path}`, 'success'); } catch (error) { notice(error.message, 'error'); } finally { button.disabled = false; button.textContent = t('runMeasurement'); } });
$('#languageButton').addEventListener('click', () => { state.language = state.language === 'en' ? 'zh' : 'en'; renderLanguage(); });
$('#settingsButton').addEventListener('click', () => $('#settingsDialog').showModal());
$('#settingsForm').addEventListener('submit', async event => { event.preventDefault(); const form = event.currentTarget; const data = Object.fromEntries(new FormData(form)); const button = $('#saveSettingsButton'); button.disabled = true; try { state.config = await api('/api/config', {method:'POST', headers:{'Content-Type':'application/json'}, body:JSON.stringify(data)}); $('#settingsDialog').close(); await Promise.all([loadConfig(), loadInputs(), loadCalibration()]); renderLanguage(); notice(state.language === 'en' ? 'Settings saved.' : '设置已保存。', 'success'); } catch (error) { notice(error.message, 'error'); } finally { button.disabled = false; } });
initialize();
