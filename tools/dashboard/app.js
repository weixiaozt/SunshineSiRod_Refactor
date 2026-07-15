const state = { language: 'en', config: null, result: null, displayMode: 'raw', compensationLog: {path:'', rows:[]} };

const text = {
  en: { eyebrow:'QUALITY INSPECTION', title:'Square Rod Measurement', settings:'Settings', dataDirectory:'Measurement data directory', measurementFile:'Measurement file', loading:'Loading files…', refresh:'Refresh', runMeasurement:'Run measurement', wholeRod:'WHOLE ROD', wholeRodResults:'Whole-rod results', awaitingResult:'Awaiting result', rodLength:'Rod length', diagonal1:'Diagonal 1', diagonal2:'Diagonal 2', diagonalDifference:'Diagonal difference', headEndFace:'Head end-face average angle', tailEndFace:'Tail end-face average angle', crossSection:'CROSS-SECTION', cornerGeometry:'Corner geometry', meanValues:'Final values = CSV mean', diagramNote:'C1–C4 follow the camera/corner mapping. Diagonals use chamfer midpoints.', edgeDimensions:'EDGE DIMENSIONS', edgeAndSymmetry:'Edge and symmetry values', calibration:'CALIBRATION', calibrationCheck:'Calibration and compensation check', pathSettings:'Paths and measurement settings', resultDirectory:'Result save directory', scriptPath:'Measurement script path', calibrationPath:'Cross-section calibration model path', endfaceCalibrationPath:'End-face calibration model path', truthPath:'Manual-truth CSV path', sliceSpacing:'Slice spacing (mm)', pathNote:'Paths are saved locally on this computer.', cancel:'Cancel', save:'Save settings', corner:'Corner', perpendicularity:'Perpendicularity', chamferLength:'Chamfer length', projectionX:'Horizontal projection (X)', projectionZ:'Height projection (Z)', camera:'Camera', model:'Model', bias:'Corner bias', manualTruth:'Manual truth check', truth:'Manual truth', measured:'Measured', difference:'Difference', noCalibration:'Calibration details are not available.', endFacePending:'Requires the four face-to-end-face angles from the measurement script.' },
  zh: { eyebrow:'质量检测', title:'方棒测量', settings:'设置', dataDirectory:'检测数据目录', measurementFile:'检测文件', loading:'正在读取文件…', refresh:'刷新', runMeasurement:'开始测量', wholeRod:'整棒数据', wholeRodResults:'整棒结果', awaitingResult:'等待测量结果', rodLength:'棒长', diagonal1:'对角线 1', diagonal2:'对角线 2', diagonalDifference:'对角线差', headEndFace:'头部端面四面平均夹角', tailEndFace:'尾部端面四面平均夹角', crossSection:'横截面', cornerGeometry:'四角几何数据', meanValues:'最终值 = CSV 平均值', diagramNote:'C1–C4 对应相机/角点编号；对角线按倒角中点计算。', edgeDimensions:'边长数据', edgeAndSymmetry:'边长和对称性', calibration:'标定', calibrationCheck:'标定与补偿核查', pathSettings:'路径和测量设置', resultDirectory:'结果保存目录', scriptPath:'测量脚本路径', calibrationPath:'横截面标定模型路径', endfaceCalibrationPath:'端面标定模型路径', truthPath:'人工真值 CSV 路径', sliceSpacing:'切片间距（mm）', pathNote:'路径会保存于本机。', cancel:'取消', save:'保存设置', corner:'角', perpendicularity:'垂直度', chamferLength:'倒角长度', projectionX:'横向投影（X）', projectionZ:'高度投影（Z）', camera:'相机', model:'模型', bias:'角点偏置', manualTruth:'人工真值核查', truth:'人工真值', measured:'当前测量', difference:'偏差', noCalibration:'当前无法读取标定信息。', endFacePending:'需要由测量脚本提供端面与四个面的夹角。' }
};
Object.assign(text.en, {
  measurementVisuals: 'MEASUREMENT VISUALS', visualGuide: 'Measurement visual guide',
  crossSectionMap: 'Edge and diagonal map', crossSectionTag: 'Top view',
  crossSectionNote: 'A–D use the delivered 75-slice calibration. D1/D2 use delivered parallel-caliper maximum support distances.',
  endFaceVisual: 'END-FACE VIEW', endFaceMap: 'Head and tail perpendicularity', sideView: 'Side view',
  head: 'Head', tail: 'Tail', rodAxis: 'Rod axis',
  endfaceNote: 'Values will appear here when the measurement script provides end-face results.',
  chamferMap: 'Chamfer, perpendicularity and projections', cornerDetail: 'Enlarged corner',
  cornerData: 'Four-corner data',
  chamferNote: 'All four corners are shown. Arc length, Projection 1/2 and the main-face angle all use the delivered geometry algorithms.'
});
Object.assign(text.zh, {
  measurementVisuals: '测量示意图', visualGuide: '测量数据可视化说明',
  crossSectionMap: '边长与对角线示意图', crossSectionTag: '横截面视图',
  crossSectionNote: 'A–D 使用交付包75截面标定；D1/D2 使用交付包的平行卡尺最大支撑距离。',
  endFaceVisual: '端面视图', endFaceMap: '头尾端面四面平均夹角', sideView: '纵向侧视图',
  head: '头部', tail: '尾部', rodAxis: '棒轴',
  endfaceNote: '测量脚本提供端面测量结果后，数值会显示在这里。',
  chamferMap: '倒角、垂直度与投影', cornerDetail: '放大角部',
  cornerData: '四角数据',
  chamferNote: '四个角全部画出；弧长、投影1/2和主面夹角全部使用新交付包算法。'
});
Object.assign(text.en, {
  headFaceAngles: 'Head: face-to-end angles',
  tailFaceAngles: 'Tail: face-to-end angles'
});
Object.assign(text.zh, {
  headFaceAngles: '\u5934\u90e8\uff1a\u56db\u9762\u4e0e\u7aef\u9762\u5939\u89d2',
  tailFaceAngles: '\u5c3e\u90e8\uff1a\u56db\u9762\u4e0e\u7aef\u9762\u5939\u89d2'
});
text.en.runMeasurement = 'Measure selected file';
text.en.endfaceNote = 'Inspect all sixteen local edge-to-ridge angles. Product values stay beside raw values; the representative angle is the actual local angle farthest from 90 degrees, not an average.';
text.zh.endfaceNote = '\u8bf7\u540c\u65f6\u770b16\u4e2a\u7aef\u9762\u68f1\u7ebf\u4e0e\u4fa7\u9762\u68f1\u7ebf\u5939\u89d2\u3002\u4fee\u6b63\u503c\u4e0eraw\u539f\u59cb\u503c\u5e76\u5217\u4fdd\u7559\uff1b\u4ee3\u8868\u503c\u662f\u5b9e\u6d4b16\u89d2\u4e2d\u504f\u79bb90\u5ea6\u6700\u5927\u7684\u90a3\u4e00\u4e2a\uff0c\u4e0d\u662f\u5e73\u5747\u503c\u3002';
text.en.headEndFace = 'Head representative local angle';
text.en.tailEndFace = 'Tail representative local angle';
text.zh.headEndFace = '\u5934\u7aef\u4ee3\u8868\u5c40\u90e8\u5939\u89d2';
text.zh.tailEndFace = '\u5c3e\u7aef\u4ee3\u8868\u5c40\u90e8\u5939\u89d2';
text.en.headFaceAngles = 'Head: eight local edge/ridge angles';
text.en.tailFaceAngles = 'Tail: eight local edge/ridge angles';
text.zh.headFaceAngles = '\u5934\u7aef\uff1a8\u4e2a\u5c40\u90e8\u68f1\u7ebf\u5939\u89d2';
text.zh.tailFaceAngles = '\u5c3e\u7aef\uff1a8\u4e2a\u5c40\u90e8\u68f1\u7ebf\u5939\u89d2';
text.zh.runMeasurement = '测量选中文件';
text.en.perpendicularity = 'Main-face angle';
text.zh.perpendicularity = '主面夹角';
text.en.chamferLength = 'Arc length';
text.zh.chamferLength = '弧长';
text.en.projectionX = 'Projection 1';
text.zh.projectionX = '投影 1';
text.en.projectionY = 'Projection 2';
text.zh.projectionY = '投影 2';
text.en.chamferMap = 'Four-corner arc, angle and projection map';
text.zh.chamferMap = '四角弧长、主面夹角与投影示意图';
text.en.chamferNote = 'The four diagrams simulate the physical top-left, top-right, bottom-left and bottom-right corners and map directly to cameras 1–4.';
text.zh.chamferNote = '四张示意图模拟左上、右上、左下、右下四个物理角，并分别对应相机1～相机4。';
text.en.endfaceNote = 'End-face values are intentionally blank until the new delivered end-face algorithm package is integrated.';
text.zh.endfaceNote = '端面数据暂时留空，等待新的端面算法交付包后再接入。';
text.en.headEndFace = 'Head end-face perpendicularity';
text.en.tailEndFace = 'Tail end-face perpendicularity';
text.zh.headEndFace = '头部端面垂直度';
text.zh.tailEndFace = '尾部端面垂直度';
text.en.headFaceAngles = 'Head: face-to-end angles';
text.en.tailFaceAngles = 'Tail: face-to-end angles';
text.zh.headFaceAngles = '头部：四面与端面夹角';
text.zh.tailFaceAngles = '尾部：四面与端面夹角';
const t = key => {
  if (state.config?.endface_only) {
    const endpointLabels = state.language === 'zh'
      ? {
          head: '设备最小行端', tail: '设备最大行端',
          headEndFace: '设备最小行端代表局部夹角', tailEndFace: '设备最大行端代表局部夹角',
          headFaceAngles: '设备最小行端：8个局部棱线夹角', tailFaceAngles: '设备最大行端：8个局部棱线夹角'
        }
      : {
          head: 'Device min-row end', tail: 'Device max-row end',
          headEndFace: 'Device min-row representative angle', tailEndFace: 'Device max-row representative angle',
          headFaceAngles: 'Device min-row end: eight local angles', tailFaceAngles: 'Device max-row end: eight local angles'
        };
    if (endpointLabels[key]) return endpointLabels[key];
  }
  return text[state.language][key] || key;
};
const $ = selector => document.querySelector(selector);
const currentSummary = () => state.result?.[state.displayMode === 'corrected' ? 'corrected_summary' : 'raw_summary'] || state.result?.summary || {};
const format = value => {
  if (value === undefined || value === null || value === '') return '—';
  const numeric = Number(value);
  return Number.isNaN(numeric) ? String(value) : numeric.toFixed(3);
};
const escapeHtml = value => String(value ?? '').replace(/[&<>"']/g, character => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[character]));

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
  renderCorners(); renderResult(); renderDriftStatus(); renderCalibrationV2(); renderEndfaceReference(); updateContinuousButton();
}
function renderCorners() {
  const mapping = [{obj:1,point:'P1',edge:'A / B'}, {obj:2,point:'P2',edge:'C / D'}, {obj:3,point:'P3',edge:'A / D'}, {obj:4,point:'P4',edge:'B / C'}];
  const summary = currentSummary();
  $('#cornerGrid').innerHTML = mapping.map((item, index) => `<article class="corner-card"><div class="corner-title"><strong>${t('corner')} ${index + 1}</strong><span>${item.point} · ${item.edge} · ${t('camera')} ${item.obj}</span></div><ul class="corner-list"><li><span>${t('perpendicularity')}</span><strong>${format(summary[`obj${item.obj}_verticality_error_deg`])} °</strong></li><li><span>${t('chamferLength')}</span><strong>${format(summary[`obj${item.obj}_chamfer_mm`])} mm</strong></li><li><span>${t('projectionX')}</span><strong>${format(summary[`obj${item.obj}_projection_x_mm`])} mm</strong></li><li><span>${t('projectionY')}</span><strong>${format(summary[`obj${item.obj}_projection_y_mm`])} mm</strong></li></ul></article>`).join('');
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
  if (!state.result) {
    $('#resultSource').textContent = t('awaitingResult');
  } else {
    const base = state.config?.endface_only
      ? `${state.result.slice_count} ${state.language === 'en' ? 'slices · CSV mean' : '个切片 · CSV 平均值'}`
      : (state.language === 'en' ? 'A/B/C/D, diagonals, arcs, projections, four angles and rod length: delivered package · end-face: pending' : 'A/B/C/D、对角线、弧长、投影、四角夹角和棒长：全部使用交付包 · 端面：等待新交付包');
    const contract = state.result.endpoint_label_contract;
    const endpointNote = contract && contract.physical_R_L_inferred === false
      ? (state.language === 'en' ? ' · device-row endpoints · physical R/L not inferred' : ' · 设备行端点 · 不推断物理R/L')
      : '';
    const rawAuditNote = state.result.endface_measurement_mode === 'raw_audit'
      ? (state.language === 'en' ? ' · RAW audit · no drift/end-face correction' : ' · Raw审计 · 未加载漂移/端面修正')
      : '';
    $('#resultSource').textContent = `${base}${endpointNote}${rawAuditNote}`;
  }
}
function renderDriftStatus() {
  const panel = $('#driftStatus');
  if (!panel) return;
  const drift = state.result?.drift;
  const relative = state.result?.relative_camera_geometry;
  const endfaceApplicability = state.result?.endface_calibration_applicability;
  const endfaceQuality = state.result?.endface_quality;
  const cn = state.language === 'zh';
  if (!drift?.drift_status) {
    panel.className = 'drift-status-panel';
    panel.innerHTML = `<strong>${cn ? '\u673a\u68b0\u6f02\u79fb' : 'Mechanical drift'}</strong><span>${cn ? '\u7b49\u5f85\u6d4b\u91cf' : 'Awaiting measurement'}</span>`;
    return;
  }
  if (!state.config?.endface_only) {
    const status = drift.drift_status;
    const labels = cn
      ? {normal:'正常，未修正', abnormal_corrected:'已命中异常漂移并修正', unmatched_unadjusted:'未命中已知漂移，保留原始结果', not_applicable_orientation:'当前方向不适用漂移模型', not_configured:'未配置漂移模型'}
      : {normal:'Normal, no correction', abnormal_corrected:'Known abnormal drift corrected', unmatched_unadjusted:'Known drift not matched; raw result retained', not_applicable_orientation:'Drift model not applicable', not_configured:'Drift model not configured'};
    panel.className = `drift-status-panel ${status === 'unmatched_unadjusted' ? 'warning' : status === 'abnormal_corrected' ? 'corrected' : 'normal'}`;
    const delivery = state.result?.delivery_abcd;
    const deliveryLabel = delivery?.applied
      ? (cn ? 'ABCD交付算法已应用（头尾各5片合并）' : 'Delivered ABCD applied (first/last 5 slices)')
      : (cn ? '等待ABCD交付算法结果' : 'Awaiting delivered ABCD result');
    const reason = drift.drift_reason ? `<small class="drift-reason">${cn ? '诊断' : 'Reason'}: ${escapeHtml(drift.drift_reason)}</small>` : '';
    panel.innerHTML = `<strong>${cn ? '机械漂移' : 'Mechanical drift'}: ${labels[status] || status}</strong><span>${deliveryLabel}</span>${reason}`;
    return;
  }
  const status = drift.drift_status;
  const labels = cn
    ? {normal:'\u6b63\u5e38\uff0c\u672a\u4fee\u6b63', abnormal_corrected:'\u5df2\u547d\u4e2d\u5f02\u5e38\u6f02\u79fb\u5e76\u4fee\u6b63', unmatched_unadjusted:'\u672a\u547d\u4e2d\u5df2\u77e5\u6f02\u79fb\uff0c\u4fdd\u7559\u539f\u59cb\u7ed3\u679c', not_applicable_orientation:'\u5f53\u524d\u65b9\u5411\u4e0d\u9002\u7528\u6f02\u79fb\u6a21\u578b', not_configured:'\u672a\u914d\u7f6e\u6f02\u79fb\u6a21\u578b'}
    : {normal:'Normal, no correction', abnormal_corrected:'Known abnormal drift corrected', unmatched_unadjusted:'Known drift not matched; raw result retained', not_applicable_orientation:'Drift model not applicable to this orientation', not_configured:'Drift model not configured'};
  const relativeWarning = String(relative?.relative_camera_geometry_warning).toLowerCase() === 'true';
  const qualityStatus = endfaceQuality?.endface_raw_quality_status || '';
  const qualityRejected = qualityStatus === 'rejected';
  const qualityWarning = qualityStatus === 'warning' || qualityStatus === 'uncertain';
  const endfaceCorrectionApplied = String(endfaceApplicability?.endface_calibration_correction_applied).toLowerCase() === 'true';
  const endfaceState = endfaceApplicability?.endface_calibration_applicability_status || '';
  const endfaceUnmatched = endfaceState && endfaceState !== 'camera_state_matched';
  panel.className = `drift-status-panel ${qualityRejected ? 'invalid' : qualityWarning || relativeWarning || endfaceUnmatched || status === 'unmatched_unadjusted' ? 'warning' : status === 'abnormal_corrected' ? 'corrected' : 'normal'}`;
  const cameraDetails = [1,2,3,4].map(obj => `<small>C${obj}: ΔX ${format(drift[`obj${obj}_drift_x_mm`])} / ΔZ ${format(drift[`obj${obj}_drift_z_mm`])} mm · k ${format(drift[`obj${obj}_drift_amplitude`])}</small>`).join('');
  const reason = drift.drift_reason ? `<small class="drift-reason">${cn ? '\u8bca\u65ad' : 'Reason'}: ${drift.drift_reason}</small>` : '';
  const seamStatus = relative?.relative_camera_geometry_status || '—';
  const seamDetail = `<small class="drift-reason">${cn ? '同面双相机诊断' : 'Same-face dual-camera audit'}: ${seamStatus} · ${cn ? '最大变化' : 'max span'} ${format(relative?.relative_camera_max_seam_span_mm)} mm · correction_applied=false</small>`;
  const applicabilityReason = endfaceApplicability?.endface_calibration_applicability_reason || '';
  const mode = state.result?.endface_measurement_mode || state.config?.endface_measurement_mode || '';
  const modeDetail = mode === 'raw_audit'
    ? `<small class="drift-reason">${cn ? '测量模式：Raw审计；不加载机械漂移模型、端面模型或机构/人工端面真值。' : 'Mode: RAW audit; no mechanical-drift model, end-face model, or institution/manual end-face truth is loaded.'}</small>`
    : '';
  const applicabilityDetail = endfaceState
    ? `<small class="drift-reason">${cn ? '端面物理标定适用性' : 'End-face physical calibration applicability'}: ${endfaceState} · correction_applied=${endfaceCorrectionApplied ? 'true' : 'false'}${applicabilityReason ? ` · ${applicabilityReason}` : ''}</small>`
    : '';
  const qualityLabel = {
    pass: cn ? '\u901a\u8fc7' : 'PASS',
    uncertain: cn ? '\u4e0d\u786e\u5b9a' : 'UNCERTAIN',
    warning: cn ? '\u544a\u8b66' : 'WARNING',
    rejected: cn ? '\u62d2\u6d4b' : 'REJECTED'
  }[qualityStatus] || qualityStatus;
  const qualityDetail = qualityStatus
    ? `<small class="drift-reason">${cn ? '\u8d28\u91cf\u95e8\u7981' : 'Quality gate'}: ${qualityLabel} · ${cn ? '\u5934/\u5c3e\u5e73\u9762RMSE' : 'head/tail plane RMSE'} ${format(endfaceQuality.head_endface_raw_plane_rmse_mm)} / ${format(endfaceQuality.tail_endface_raw_plane_rmse_mm)} mm · accepted=${escapeHtml(endfaceQuality.endface_raw_quality_accepted)} · truth=false · correction=false${endfaceQuality.endface_raw_quality_reason ? ` · ${escapeHtml(endfaceQuality.endface_raw_quality_reason)}` : ''}</small>`
    : '';
  const heading = mode === 'raw_audit' && qualityStatus
    ? `${cn ? 'Raw\u7aef\u9762\u53ef\u4fe1\u72b6\u6001' : 'Raw end-face quality'}: ${qualityLabel}`
    : `${cn ? '\u673a\u68b0\u6f02\u79fb' : 'Mechanical drift'}: ${labels[status] || status}`;
  const primaryMetrics = mode === 'raw_audit'
    ? `${cn ? '\u540c\u9762\u6700\u5927\u62fc\u63a5\u53d8\u5316' : 'Maximum same-face seam span'} ${format(relative?.relative_camera_max_seam_span_mm)} mm`
    : `${cn ? '\u5e45\u5ea6' : 'Amplitude'} ${format(drift.drift_amplitude)} · ${cn ? '\u7f6e\u4fe1\u5ea6' : 'Confidence'} ${format(Number(drift.drift_confidence) * 100)}% · RMSE ${format(drift.drift_fit_rmse_mm)} mm · ${cn ? '\u91cd\u53e0\u7ad9\u70b9' : 'Overlap stations'} ${format(drift.drift_sample_station_count)} (${format(Number(drift.drift_overlap_start_fraction) * 100)}–${format(Number(drift.drift_overlap_end_fraction) * 100)}%) · v${drift.drift_model_version || '—'}`;
  panel.innerHTML = `<strong>${heading}</strong><span>${primaryMetrics}</span>${qualityDetail}${modeDetail}${reason}${seamDetail}${applicabilityDetail}<div class="drift-camera-values">${cameraDetails}</div>`;
}
function renderEndfaceReference() {
  const panel = $('#endfaceReferencePanel');
  if (!panel) return;
  const cal = state.calibration || {};
  const truth = cal.endface_truth_angles_deg || {};
  const means = cal.endface_truth_means_deg || {};
  panel.querySelectorAll('[data-calibration-field]').forEach(el => {
    const key = el.dataset.calibrationField;
    if (key.endsWith('_mean')) {
      el.textContent = format(means[key.slice(0, -5)]);
      return;
    }
    const [end, face] = key.split('_');
    el.textContent = format(truth[end]?.[face]);
  });
  const cn = state.language === 'zh';
  $('#endfaceReferenceEyebrow').textContent = cn ? '\u6807\u5b9a\u6807\u51c6\u503c' : 'CALIBRATION REFERENCE';
  $('#endfaceReferenceTitle').textContent = cn ? '\u4e13\u4e1a\u673a\u6784\u7aef\u9762\u5939\u89d2' : 'Professional institution values';
  $('#headReferenceAverageLabel').textContent = cn ? '\u5934\u90e8\u56db\u9762\u5e73\u5747\u5939\u89d2' : 'Head average angle';
  $('#tailReferenceAverageLabel').textContent = cn ? '\u5c3e\u90e8\u56db\u9762\u5e73\u5747\u5939\u89d2' : 'Tail average angle';
  $('#headReferenceAnglesLabel').textContent = cn ? '\u5934\u90e8\uff1a\u56db\u9762\u4e0e\u7aef\u9762\u5939\u89d2' : 'Head: face-to-end angles';
  $('#tailReferenceAnglesLabel').textContent = cn ? '\u5c3e\u90e8\uff1a\u56db\u9762\u4e0e\u7aef\u9762\u5939\u89d2' : 'Tail: face-to-end angles';
  $('#endfaceReferenceNote').textContent = cn
    ? '\u53f3\u4fa7\u4ec5\u5c55\u793a\u673a\u6784R\u5934/L\u5c3e\u53c2\u8003\u503c\uff0c\u8fd0\u884c\u65f6\u7edd\u4e0d\u7528\u4e8e\u5339\u914d\u65b9\u5411\u6216\u62c9\u56de\u7ed3\u679c\u3002\u673a\u6784\u9762\u53f7A\u4e0a\u3001B\u53f3\u3001C\u5de6\u3001D\u4e0b\u3002head/tail\u56fa\u5b9a\u8868\u793aHOBJ\u8bbe\u5907\u7a7a\u95f4\u6700\u5c0f\u884c/\u6700\u5927\u884c\uff0c\u4e0d\u662f\u7535\u673a\u8fd0\u52a8\u8d77\u70b9/\u7ec8\u70b9\u3002\u7269\u7406\u8c03\u5934\u5fc5\u987b\u7531\u56fe\u50cf\u81ea\u7136\u4ea7\u751fhead\u2194tail\u3001B\u2194C\u3001A/D\u4e0d\u53d8\uff1b\u7a0b\u5e8f\u4e0d\u731c\u7269\u7406R/L\u3002'
    : 'The right side is reference-only and is never used to choose orientation or pull a result toward truth. Institution faces are A=top, B=right, C=left and D=bottom. Head/tail are fixed HOBJ device-row minimum/maximum, not motor travel start/end. A verified physical turnover must emerge from geometry as head/tail and B/C exchange (A/D fixed); the program never guesses physical R/L.';
  const details = [cal.endface_nominal_spec, cal.endface_sample_id, cal.endface_version ? `v${cal.endface_version}` : ''].filter(Boolean);
  $('#endfaceReferenceMeta').textContent = cal.endface_available && Object.keys(truth).length
    ? details.join(' \u00b7 ')
    : (cn ? '\u8bf7\u5148\u7528\u73b0\u573a\u6807\u5b9a\u7a0b\u5e8f\u751f\u6210\u7aef\u9762\u6a21\u578b' : 'Generate and activate an end-face model first');
}
function renderCompensationLog() {
  const host = $('#manualCompensationHost');
  if (!host || state.config?.endface_only) return;
  host.querySelector('#compensationLogPanel')?.remove();
  const cn = state.language === 'zh';
  const log = state.compensationLog || {path:'', rows:[]};
  const rows = Array.isArray(log.rows) ? log.rows : [];
  const entries = rows.length
    ? rows.map(row => {
        const raw = row.latest_raw_measurement;
        const display = raw !== ''
          ? `<p>${cn ? '显示值' : 'Display'}: ${escapeHtml(row.display_before_change)} → ${escapeHtml(row.display_after_change)} mm (${cn ? '原始测量' : 'raw'} ${escapeHtml(raw)} mm)</p>`
          : `<p>${cn ? '当前没有测量结果，仅记录补偿参数。' : 'No current measurement; compensation parameters only.'}</p>`;
        return `<article class="compensation-log-entry"><header><b>${escapeHtml(row.item)}</b><time>${escapeHtml(row.changed_at)}</time></header><p>${cn ? '补偿参数' : 'Offset'}: ${escapeHtml(row.original_compensation)} → ${escapeHtml(row.new_compensation)} mm</p>${display}</article>`;
      }).join('')
    : `<p class="compensation-log-empty">${cn ? '还没有补偿修改记录。' : 'No compensation changes yet.'}</p>`;
  host.insertAdjacentHTML('beforeend', `<section id="compensationLogPanel" class="compensation-log-panel"><h4>${cn ? '补偿修改日志' : 'Compensation change log'}</h4><small class="compensation-log-path">${escapeHtml(log.path || '')}</small><div class="compensation-log-list">${entries}</div></section>`);
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
  if (!state.config?.endface_only) {
    const host = $('#manualCompensationHost');
    const offsets = state.config?.edge_offsets_mm || {};
    const diagonalOffsets = state.config?.diagonal_offsets_mm || {diag1:0, diag2:0};
    const lengthOffset = Number(state.config?.length_offset_mm ?? 0);
    const inputs = [
      ...['A','B','C','D'].map(edge => ({label:`${cn ? '边' : 'Edge '}${edge}`, name:`offset_${edge}`, value:offsets[edge]})),
      {label:cn ? '对角线1' : 'Diagonal 1', name:'offset_diag1', value:diagonalOffsets.diag1},
      {label:cn ? '对角线2' : 'Diagonal 2', name:'offset_diag2', value:diagonalOffsets.diag2},
      {label:cn ? '棒长' : 'Rod length', name:'offset_length', value:lengthOffset},
    ].map(item => `<label>${item.label}<input name="${item.name}" type="number" step="any" value="${Number(item.value || 0)}"></label>`).join('');
    const mode = state.displayMode;
    $('#modelVersion').textContent = cn ? '交付包几何算法' : 'Delivered geometry';
    container.innerHTML = '';
    host.innerHTML = `<section class="manual-compensation"><p class="eyebrow">${cn ? '7项显示补偿' : '7 DISPLAY OFFSETS'}</p><div class="mode-toggle"><button type="button" data-summary-mode="raw" class="${mode === 'raw' ? 'active' : ''}">${cn ? '原始值' : 'Raw'}</button><button type="button" data-summary-mode="corrected" class="${mode === 'corrected' ? 'active' : ''}">${cn ? '补偿后' : 'Corrected'}</button></div><form id="manualCompensationForm"><p class="offset-section-title">${cn ? '边长、对角线与棒长补偿 (mm)' : 'EDGE, DIAGONAL AND LENGTH OFFSETS (mm)'}</p><div class="offset-inputs">${inputs}</div><button class="primary-button" type="submit">${cn ? '保存补偿' : 'Save compensation'}</button></form><p>${cn ? '原始值始终保留；补偿只作用于A/B/C/D、两条对角线和棒长。端面补偿永久禁用。' : 'Raw values are always retained. Offsets apply only to A/B/C/D, two diagonals and rod length. End-face offsets remain disabled.'}</p></section>`;
    host.querySelectorAll('[data-summary-mode]').forEach(button => button.addEventListener('click', () => { state.displayMode = button.dataset.summaryMode; renderLanguage(); }));
    $('#manualCompensationForm').addEventListener('submit', async event => {
      event.preventDefault();
      const form = event.currentTarget;
      const edge_offsets_mm = {};
      const diagonal_offsets_mm = {};
      const length_offset_mm = Number(form.elements.offset_length.value || 0);
      ['A','B','C','D'].forEach(edge => edge_offsets_mm[edge] = Number(form.elements[`offset_${edge}`].value || 0));
      ['diag1','diag2'].forEach(diagonal => diagonal_offsets_mm[diagonal] = Number(form.elements[`offset_${diagonal}`].value || 0));
      try {
        state.config = await api('/api/config', {method:'POST', headers:{'Content-Type':'application/json'}, body:JSON.stringify({edge_offsets_mm, diagonal_offsets_mm, length_offset_mm})});
        if (state.result?.raw_summary) {
          const raw = state.result.raw_summary;
          const corrected = {...raw};
          ['A','B','C','D'].forEach(edge => {
            const key = `${edge}_mm`;
            if (raw[key] !== '' && raw[key] !== undefined) corrected[key] = (Number(raw[key]) + edge_offsets_mm[edge]).toFixed(6);
          });
          [['diag1','diag1_M1_M2_mm'], ['diag2','diag2_M3_M4_mm']].forEach(([diagonal, key]) => {
            if (raw[key] !== '' && raw[key] !== undefined) corrected[key] = (Number(raw[key]) + diagonal_offsets_mm[diagonal]).toFixed(6);
          });
          if (raw.stick_length_mm !== '' && raw.stick_length_mm !== undefined) corrected.stick_length_mm = (Number(raw.stick_length_mm) + length_offset_mm).toFixed(6);
          state.result.corrected_summary = corrected;
        }
        state.displayMode = 'corrected';
        await loadCompensationLog();
        renderLanguage();
        notice(cn ? '补偿已保存，页面已切换到补偿后数值。' : 'Compensation saved; corrected values are displayed.', 'success');
      } catch (error) {
        notice(error.message, 'error');
      }
    });
    renderCompensationLog();
    return;
  }
  if (!cal?.available) { container.innerHTML = `<p class="empty-state">${cal?.message || 'Calibration details are not available.'}</p>`; $('#manualCompensationHost').innerHTML = ''; return; }
  if (state.config?.endface_only) {
    $('#modelVersion').textContent = cal.endface_available
      ? `${cn ? '\u7aef\u9762\u7269\u7406\u6807\u5b9a' : 'Physical end-face calibration'} v${cal.endface_version}`
      : (cn ? '\u5f53\u524d\u542f\u7528M0 Raw\uff1a\u65e0\u5df2\u653e\u884cv15 M1\u4fee\u6b63' : 'M0 RAW ACTIVE: no release-ready v15 M1 correction');
    const wireframe = cal.endface_wireframe_validation || {};
    const fixed = wireframe.fixed_blind_holdout || {};
    const fixedStats = fixed.statistics || {};
    const cross = wireframe.paired_cross_validation || {};
    const crossStats = cross.statistics || {};
    const readiness = cal.endface_release_readiness || {};
    const quality = cal.endface_capture_geometry_quality || {};
    const comparison = cal.endface_model_comparison || {};
    const external = cal.endface_external_noncalibration_bar_validation || {};
    const failures = Array.isArray(cal.endface_failures) ? cal.endface_failures : [];
    const maximumFinite = (...values) => {
      const finite = values.map(Number).filter(Number.isFinite);
      return finite.length ? Math.max(...finite) : undefined;
    };
    const fixedGateRms = maximumFinite(fixedStats.group_median_rmse_deg, fixedStats.ordinal_pair_rmse_deg);
    const fixedGateMax = maximumFinite(fixedStats.group_median_max_abs_error_deg, fixedStats.ordinal_pair_max_abs_error_deg);
    const crossGateRms = maximumFinite(crossStats.physical_corrected_group_median_rmse_deg, crossStats.physical_corrected_ordinal_pair_rmse_deg);
    const crossGateMax = maximumFinite(crossStats.physical_corrected_group_median_max_abs_error_deg, crossStats.physical_corrected_ordinal_pair_max_abs_error_deg);
    const passLabel = value => value === true ? (cn ? '\u901a\u8fc7' : 'PASS') : (cn ? '\u672a\u901a\u8fc7' : 'BLOCKED');
    const comparisonCard = level => {
      const candidate = comparison[level] || {};
      const fixedCandidate = candidate.fixed_blind_holdout || {};
      const fixedCandidateStats = fixedCandidate.statistics || {};
      const crossCandidateStats = candidate.paired_cross_validation?.statistics || {};
      const status = level === 'M0'
        ? (cn ? '\u6b63\u5f0fRaw\u57fa\u7ebf' : 'ACTIVE RAW BASELINE')
        : level === 'M2'
          ? (cn ? '\u4ec5\u8bca\u65ad\uff0c\u7981\u6b62\u8fd0\u884c' : 'DIAGNOSTIC ONLY')
          : passLabel(candidate.passed);
      return `<div><p class="eyebrow">${level}</p><table><tbody><tr><th>${cn ? '\u72b6\u6001' : 'Status'}</th><td>${status}</td></tr><tr><th>${cn ? '\u56fa\u5b9a\u76f2\u6d4bRMS' : 'Fixed RMS'}</th><td>${format(fixedCandidateStats.ordinal_pair_rmse_deg)} &deg;</td></tr><tr><th>${cn ? '\u56fa\u5b9a\u6700\u5927\u8bef\u5dee' : 'Fixed max'}</th><td>${format(fixedCandidateStats.ordinal_pair_max_abs_error_deg)} &deg;</td></tr><tr><th>${cn ? '\u4ea4\u53c9\u9a8c\u8bc1RMS' : 'CV RMS'}</th><td>${format(crossCandidateStats.physical_corrected_ordinal_pair_rmse_deg)} &deg;</td></tr><tr><th>${cn ? '\u53c2\u6570\u6ce2\u52a8' : 'Parameter spread'}</th><td>${format(crossCandidateStats.maximum_parameter_range_mm)} mm</td></tr></tbody></table></div>`;
    };
    const modelComparisonHtml = `<div class="calibration-three-grid endface-gate-grid endface-model-comparison">${comparisonCard('M0')}${comparisonCard('M1')}${comparisonCard('M2')}</div>`;
    const externalHtml = external.status === 'completed'
      ? `<div class="calibration-overview"><div class="calibration-item"><span>${cn ? 'CTB\u5916\u90e8\u9a8c\u8bc1' : 'CTB external validation'}</span><strong>${passLabel(external.passed)}</strong></div><div class="calibration-item"><span>${cn ? '\u672a\u91cd\u65b0\u62df\u5408' : 'No refit'}</span><strong>${external.model_refit_on_ctb === false ? 'true' : 'false'}</strong></div><div class="calibration-item"><span>${cn ? '\u8c03\u5934RMS Raw\u2192M1' : 'Turnover RMS Raw\u2192M1'}</span><strong>${format(external.raw_turnover_equivariance_rms_deg)}\u2192${format(external.fixed_m1_turnover_equivariance_rms_deg)}&deg;</strong></div><div class="calibration-item"><span>${cn ? '\u62fc\u63a5RMS Raw\u2192M1' : 'Seam RMS Raw\u2192M1'}</span><strong>${format(external.raw_same_edge_seam_rms_mm)}\u2192${format(external.fixed_m1_same_edge_seam_rms_mm)} mm</strong></div></div>`
      : '';
    const failureItems = failures.length
      ? failures.map(value => `<li>${escapeHtml(value)}</li>`).join('')
      : `<li>${cn ? '\u65e0' : 'None'}</li>`;
    container.innerHTML = `<div class="calibration-overview"><div class="calibration-item"><span>${cn ? '\u5019\u9009\u6a21\u578b' : 'Candidate model'}</span><strong>v${escapeHtml(cal.endface_version)}</strong></div><div class="calibration-item"><span>${cn ? '\u603b\u653e\u884c\u72b6\u6001' : 'Overall release'}</span><strong>${passLabel(readiness.ready)}</strong></div><div class="calibration-item"><span>${cn ? '16\u89d2\u5171\u4eab\u51e0\u4f55' : 'Shared 16-angle geometry'}</span><strong>${passLabel(wireframe.passed)}</strong></div><div class="calibration-item"><span>${cn ? '\u76f8\u673a\u72b6\u6001\u5168\u7a33\u5b9a' : 'All camera states stable'}</span><strong>${passLabel(cal.endface_camera_state_all_stable)}</strong></div></div><div class="calibration-three-grid endface-gate-grid"><div><p class="eyebrow">${cn ? '\u56fa\u5b9a\u76f2\u6d4b' : 'FIXED BLIND HOLDOUT'}</p><table><tbody><tr><th>${cn ? '\u72b6\u6001' : 'Status'}</th><td>${passLabel(fixed.passed)}</td></tr><tr><th>RMS</th><td>${format(fixedGateRms)} &deg;</td></tr><tr><th>${cn ? '\u6700\u5927\u8bef\u5dee' : 'Max error'}</th><td>${format(fixedGateMax)} &deg;</td></tr><tr><th>${cn ? '\u91cd\u590d\u8303\u56f4' : 'Repeatability range'}</th><td>${format(fixedStats.maximum_repeatability_range_deg)} &deg;</td></tr></tbody></table></div><div><p class="eyebrow">${cn ? '5\u6298\u4ea4\u53c9\u9a8c\u8bc1' : 'PAIRED CROSS-VALIDATION'}</p><table><tbody><tr><th>${cn ? '\u72b6\u6001' : 'Status'}</th><td>${passLabel(cross.passed)}</td></tr><tr><th>RMS</th><td>${format(crossGateRms)} &deg;</td></tr><tr><th>${cn ? '\u6700\u5927\u8bef\u5dee' : 'Max error'}</th><td>${format(crossGateMax)} &deg;</td></tr><tr><th>${cn ? '\u91cd\u590d\u8303\u56f4' : 'Repeatability range'}</th><td>${format(crossStats.physical_corrected_maximum_repeatability_range_deg)} &deg;</td></tr><tr><th>${cn ? '\u53c2\u6570\u6ce2\u52a8' : 'Parameter spread'}</th><td>${format(crossStats.maximum_parameter_range_mm)} mm</td></tr></tbody></table></div><div><p class="eyebrow">${cn ? '\u56fe\u50cfQC' : 'IMAGE QC'}</p><table><tbody><tr><th>${cn ? '\u603b\u56fe\u6570' : 'Captures'}</th><td>${format(quality.per_capture?.length)}</td></tr><tr><th>${cn ? '\u5254\u9664' : 'Excluded'}</th><td>${format(quality.excluded_captures?.length)}</td></tr><tr><th>${cn ? '\u6700\u7ec8\u89d2\u8865\u507f' : 'Final angle offsets'}</th><td>false</td></tr><tr><th>${cn ? '\u8fd0\u884c\u65f6\u8bfb\u771f\u503c' : 'Runtime truth lookup'}</th><td>false</td></tr></tbody></table></div></div><div class="endface-gate-failures"><p class="eyebrow">${cn ? '\u62d2\u7edd\u539f\u56e0' : 'RELEASE BLOCKERS'}</p><ul>${failureItems}</ul><small>${escapeHtml(cal.endface_rejection_reason || '')}</small></div>`;
    container.insertAdjacentHTML('afterbegin', modelComparisonHtml);
    container.insertAdjacentHTML('afterbegin', externalHtml);
    $('#manualCompensationHost').innerHTML = '';
    return;
  }
  $('#modelVersion').textContent = `${cn ? '\u6807\u5b9a' : 'Calibration'} v${cal.version}`;
  const standardsByCapture = cal.standards_by_capture && Object.keys(cal.standards_by_capture).length ? cal.standards_by_capture : null;
  const standardEntries = standardsByCapture
    ? Object.entries(standardsByCapture).flatMap(([barId, positions]) => Object.entries(positions || {}).map(([name, value]) => ({barId, name, value})))
    : Object.entries(cal.standard || {}).map(([name, value]) => ({barId:'—', name, value}));
  const standardRows = standardEntries.map(({barId, name, value}) => `<tr><td>${barId}</td><td>${name}</td><td>${format(value.A)}</td><td>${format(value.B)}</td><td>${format(value.C)}</td><td>${format(value.D)}</td></tr>`).join('') || '<tr><td colspan="6">—</td></tr>';
  const calibrationBars = standardsByCapture ? Object.keys(standardsByCapture).length : (standardEntries.length ? 1 : 0);
  const calibrationSlices = standardEntries.length;
  const biasRows = Object.entries(cal.corner_biases || {}).map(([point, value]) => `<tr><td>${point}</td><td>${format(value[0])}</td><td>${format(value[1])}</td></tr>`).join('') || '<tr><td colspan="3">—</td></tr>';
  const offsets = state.config?.edge_offsets_mm || cal.manual_edge_offsets_mm || {};
  const diagonalOffsets = state.config?.diagonal_offsets_mm || {diag1:0, diag2:0};
  const lengthOffset = Number(state.config?.length_offset_mm ?? cal.manual_length_offset_mm ?? 0);
  const offsetInputs = [
    ...['A','B','C','D'].map(edge => ({label:edge, name:`offset_${edge}`, value:offsets[edge]})),
    {label:'D1', name:'offset_diag1', value:diagonalOffsets.diag1},
    {label:'D2', name:'offset_diag2', value:diagonalOffsets.diag2},
    {label:cn ? '\u68d2\u957f' : 'Length', name:'offset_length', value:lengthOffset}
  ].map(item => `<label>${item.label}<input name="${item.name}" type="number" step="any" value="${Number(item.value || 0)}"></label>`).join('');
  const mode = state.displayMode;
  container.innerHTML = `<div class="calibration-overview"><div class="calibration-item"><span>${cn ? '\u6a21\u578b' : 'Model'}</span><strong>${cal.model}</strong></div><div class="calibration-item"><span>${cn ? '\u6807\u5b9a\u7248\u672c' : 'Calibration version'}</span><strong>v${cal.version}</strong></div><div class="calibration-item"><span>${cn ? '\u8054\u5408\u6807\u5b9a' : 'Joint calibration'}</span><strong>${calibrationBars} ${cn ? '\u6839\u68d2\u00b7' : 'bars · '}${calibrationSlices} ${cn ? '\u4e2a\u622a\u9762\u70b9' : 'cross-sections'}</strong></div><div class="calibration-item"><span>${cn ? '\u6a21\u578b\u8def\u5f84' : 'Model path'}</span><strong>${cal.path}</strong></div></div><div class="calibration-three-grid"><div><p class="eyebrow">${cn ? '\u6807\u5b9a\u771f\u503c' : 'CALIBRATION TRUTH'}</p><div class="table-wrap"><table><thead><tr><th>${cn ? '\u6807\u51c6\u68d2' : 'Bar'}</th><th>${cn ? '\u4f4d\u7f6e' : 'Position'}</th><th>A</th><th>B</th><th>C</th><th>D</th></tr></thead><tbody>${standardRows}</tbody></table></div></div><div><p class="eyebrow">${cn ? '\u89d2\u70b9\u6807\u5b9a\u504f\u7f6e' : 'CORNER BIAS'}</p><div class="table-wrap"><table><thead><tr><th>Point</th><th>ΔX mm</th><th>ΔZ mm</th></tr></thead><tbody>${biasRows}</tbody></table></div></div><div class="manual-compensation"><p class="eyebrow">${cn ? '\u4eba\u5de5\u8865\u507f' : 'MANUAL COMPENSATION'}</p><div class="mode-toggle"><button type="button" data-summary-mode="raw" class="${mode === 'raw' ? 'active' : ''}">${cn ? '\u539f\u59cb\u503c' : 'Raw'}</button><button type="button" data-summary-mode="corrected" class="${mode === 'corrected' ? 'active' : ''}">${cn ? '\u8865\u507f\u540e' : 'Corrected'}</button></div><form id="manualCompensationForm"><p class="offset-section-title">${cn ? '\u8fb9\u957f\u8865\u507f (mm)' : 'EDGE OFFSETS (mm)'}</p><div class="offset-inputs">${offsetInputs}</div><button class="primary-button" type="submit">${cn ? '\u4fdd\u5b58\u8865\u507f' : 'Save compensation'}</button></form><p>${cn ? '\u53ea\u5141\u8bb8\u8fb9\u957f\u3001\u5bf9\u89d2\u7ebf\u548c\u68d2\u957f\u663e\u793a\u8865\u507f\uff1b\u7aef\u9762\u6700\u7ec8\u89d2\u8865\u507f\u6c38\u4e45\u7981\u7528\u3002' : 'Only dimensional display offsets are allowed; final end-face angle offsets are permanently disabled.'}</p></div></div>`;
  const compensationHost = $('#manualCompensationHost');
  const compensationPanel = container.querySelector('.manual-compensation');
  compensationHost.replaceChildren(compensationPanel);
  compensationHost.querySelector('.offset-section-title').textContent = cn ? '边长、对角线与棒长补偿 (mm)' : 'EDGE, DIAGONAL AND ROD-LENGTH OFFSETS (mm)';
  compensationHost.querySelector('.manual-compensation > p:last-child').textContent = cn ? '只允许边长、对角线和棒长补偿；端面最终角度补偿永久禁用，所有端面值必须来自当前HOBJ几何。' : 'Only edge, diagonal and rod-length offsets are allowed; final end-face angle offsets are permanently disabled.';
  compensationHost.querySelectorAll('[data-summary-mode]').forEach(button => button.addEventListener('click', () => { state.displayMode = button.dataset.summaryMode; renderLanguage(); }));
  $('#manualCompensationForm').addEventListener('submit', async event => {
    event.preventDefault();
    const form = event.currentTarget;
    const edge_offsets_mm = {};
    const diagonal_offsets_mm = {};
    const length_offset_mm = Number(form.elements.offset_length.value || 0);
    ['A','B','C','D'].forEach(edge => edge_offsets_mm[edge] = Number(form.elements[`offset_${edge}`].value || 0));
    ['diag1','diag2'].forEach(diagonal => diagonal_offsets_mm[diagonal] = Number(form.elements[`offset_${diagonal}`].value || 0));
    try {
      state.config = await api('/api/config', {method:'POST', headers:{'Content-Type':'application/json'}, body:JSON.stringify({edge_offsets_mm, diagonal_offsets_mm, length_offset_mm})});
      if (state.result?.raw_summary) {
        const raw = state.result.raw_summary;
        const corrected = {...raw};
        ['A','B','C','D'].forEach(edge => {
          const key = `${edge}_mm`;
          if (raw[key] !== '' && raw[key] !== undefined) corrected[key] = (Number(raw[key]) + edge_offsets_mm[edge]).toFixed(6);
        });
        [['diag1','diag1_M1_M2_mm'], ['diag2','diag2_M3_M4_mm']].forEach(([diagonal, key]) => {
          if (raw[key] !== '' && raw[key] !== undefined) corrected[key] = (Number(raw[key]) + diagonal_offsets_mm[diagonal]).toFixed(6);
        });
        if (raw.stick_length_mm !== '' && raw.stick_length_mm !== undefined) corrected.stick_length_mm = (Number(raw.stick_length_mm) + length_offset_mm).toFixed(6);
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

async function loadInputs() {
  const data = await api('/api/inputs');
  const select = $('#fileSelect');
  select.innerHTML = data.files.length ? data.files.map(file => `<option value="${file.path}">[${file.kind.toUpperCase()}] ${file.relative} · ${file.modified.replace('T', ' ')}</option>`).join('') : `<option value="">${state.language === 'en' ? 'No HOBJ or TIFF sets found' : '未找到 HOBJ 或 TIFF 图组'}</option>`;
}
function updateContinuousButton() {
  const button = $('#continuousButton');
  if (!button || !state.config) return;
  const running = Boolean(state.config.continuous_measure_enabled);
  button.textContent = running ? (state.language === 'zh' ? '停止连续测量' : 'Stop continuous measurement') : (state.language === 'zh' ? '开始连续测量' : 'Start continuous measurement');
  button.classList.toggle('auto-active', running);
}
async function loadConfig() {
  state.config = await api('/api/config');
  document.body.classList.toggle('endface-only', Boolean(state.config.endface_only));
  document.body.classList.toggle('endface-raw-only', Boolean(state.config.endface_raw_only));
  document.body.classList.toggle('endface-dashboard', Boolean(state.config.endface_only));
  document.body.classList.toggle('global-dashboard', !state.config.endface_only);
  const modeBadge = $('#measurementModeBadge');
  if (state.config.endface_only) {
    document.title = 'End-face Perpendicularity Dashboard';
    text.en.title = 'End-face Perpendicularity Measurement';
    text.zh.title = '\u7aef\u9762\u5782\u76f4\u5ea6\u68c0\u6d4b';
    text.en.visualGuide = 'End-face measurement results';
    text.zh.visualGuide = '\u7aef\u9762\u4e0e\u56db\u9762\u5939\u89d2';
    const rawAudit = state.config.endface_measurement_mode === 'raw_audit';
    modeBadge.textContent = rawAudit
      ? (state.language === 'zh' ? 'Raw审计：无修正' : 'RAW audit: unadjusted')
      : (state.language === 'zh' ? '已放行物理修正' : 'Release-corrected');
    modeBadge.classList.toggle('raw-audit', rawAudit);
    modeBadge.classList.toggle('release-corrected', !rawAudit);
  } else {
    document.title = 'Square Rod Global Measurement Dashboard';
    modeBadge.textContent = state.language === 'zh' ? '全局检测' : 'Global measurement';
  }
  $('#dataRoot').textContent = state.config.data_root;
  const form = $('#settingsForm');
  Object.keys(state.config)
    .filter(key => form.elements[key])
    .forEach(key => form.elements[key].value = state.config[key]);
  updateContinuousButton();
}
async function loadCalibration() { state.calibration = await api('/api/calibration'); renderCalibrationV2(); }
async function loadCompensationLog() {
  try {
    state.compensationLog = await api('/api/compensation-log');
  } catch (_) {
    state.compensationLog = {path:'', rows:[]};
  }
  renderCompensationLog();
}
async function loadLatestResult() { try { const data = await api('/api/latest-result'); if (data.available && data.result?.csv_path && data.result.csv_path !== state.result?.csv_path) { state.result = data.result; renderLanguage(); notice(state.language === 'en' ? `Continuous measurement complete: ${data.result.csv_path}` : `连续测量完成：${data.result.csv_path}`, 'success'); } } catch (_) {} }
async function initialize() { try { await loadConfig(); await Promise.all([loadInputs(), loadCalibration(), loadLatestResult(), loadCompensationLog()]); renderLanguage(); } catch (error) { notice(error.message, 'error'); } }
$('#refreshButton').addEventListener('click', async () => { notice(t('loading')); try { await loadInputs(); notice(state.language === 'en' ? 'File list refreshed.' : '文件列表已刷新。', 'success'); } catch (error) { notice(error.message, 'error'); } });
$('#continuousButton').addEventListener('click', async () => { const button = $('#continuousButton'); button.disabled = true; try { const enabled = !Boolean(state.config?.continuous_measure_enabled); state.config = await api('/api/config', {method:'POST', headers:{'Content-Type':'application/json'}, body:JSON.stringify({continuous_measure_enabled: enabled})}); updateContinuousButton(); notice(enabled ? (state.language === 'en' ? 'Continuous measurement started. Existing unprocessed HOBJ files will run once; new or updated files will run when stable.' : '连续测量已开始：未处理的 HOBJ 会各运行一次；新增或更新文件稳定后会自动运行。') : (state.language === 'en' ? 'Continuous measurement stopped. The current file, if any, may finish; no further files will start.' : '连续测量已停止。当前正在测量的文件可能会完成，但不会再启动后续文件。'), 'success'); } catch (error) { notice(error.message, 'error'); } finally { button.disabled = false; } });
$('#measureButton').addEventListener('click', async () => { const input_path = $('#fileSelect').value; if (!input_path) { notice(state.language === 'en' ? 'Select a measurement file first.' : '请先选择检测文件。', 'error'); return; } const button = $('#measureButton'); button.disabled = true; button.textContent = state.language === 'en' ? 'Measuring…' : '正在测量…'; notice(state.language === 'en' ? 'The measurement is running. Large HOBJ files may take a moment.' : '正在运行检测，大型 HOBJ 文件可能需要一些时间。'); try { state.result = await api('/api/measure', {method:'POST', headers:{'Content-Type':'application/json'}, body:JSON.stringify({input_path})}); renderLanguage(); notice(state.language === 'en' ? `Measurement complete. Slice CSV: ${state.result.csv_path}; statistics CSV: ${state.result.statistics_csv_path}` : `检测完成。切片 CSV：${state.result.csv_path}；统计 CSV：${state.result.statistics_csv_path}`, 'success'); } catch (error) { notice(error.message, 'error'); } finally { button.disabled = false; button.textContent = t('runMeasurement'); } });
$('#languageButton').addEventListener('click', () => { state.language = state.language === 'en' ? 'zh' : 'en'; renderLanguage(); });
$('#settingsButton').addEventListener('click', () => $('#settingsDialog').showModal());
$('#settingsForm').addEventListener('submit', async event => { event.preventDefault(); const form = event.currentTarget; const data = Object.fromEntries(new FormData(form)); const button = $('#saveSettingsButton'); button.disabled = true; try { state.config = await api('/api/config', {method:'POST', headers:{'Content-Type':'application/json'}, body:JSON.stringify(data)}); $('#settingsDialog').close(); await Promise.all([loadConfig(), loadInputs(), loadCalibration(), loadCompensationLog()]); renderLanguage(); notice(state.language === 'en' ? 'Settings saved.' : '设置已保存。', 'success'); } catch (error) { notice(error.message, 'error'); } finally { button.disabled = false; } });
initialize();
setInterval(loadLatestResult, 3000);
