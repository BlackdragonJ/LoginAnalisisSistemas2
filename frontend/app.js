const API_BASE = "http://localhost:5000/api"; 

function getToken(){ return localStorage.getItem("token") || ""; }
function getRol(){ return localStorage.getItem("rol") || ""; }
function setAuth(token, rol){ localStorage.setItem("token", token); localStorage.setItem("rol", rol); }
function clearAuth(){ localStorage.removeItem("token"); localStorage.removeItem("rol"); localStorage.removeItem("usuario"); }
function logout(){ clearAuth(); location.href = "login.html"; }

async function apiGet(path){
  const r = await fetch(`${API_BASE}${path}`, {
    headers: { Authorization: `Bearer ${getToken()}` }
  });
  const data = await r.json().catch(()=> ({}));
  if(!r.ok) throw new Error(data?.error || `${r.status} ${r.statusText}`);
  return data;
}

async function apiSend(method, path, body){
  const r = await fetch(`${API_BASE}${path}`, {
    method,
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${getToken()}`
    },
    body: JSON.stringify(body || {})
  });
  const data = await r.json().catch(()=> ({}));
  if(!r.ok) throw new Error(data?.error || `${r.status} ${r.statusText}`);
  return data;
}

async function cargar(){
  const msg = document.getElementById("listMsg");
  msg.textContent = "";
  try{
    let data = await apiGet("/user");

    if(getRol() === "Desarrollador"){
      data = data.filter(u => u.nombre_rol === "Desarrollador");
    }

    const tbody = document.querySelector("#tabla tbody");
    tbody.innerHTML = "";

    data.forEach(u => {
      const tr = document.createElement("tr");
      tr.innerHTML = `
        <td>${u.id_usuario}</td>
        <td>${escapeHtml(u.usuario || "")}</td>
        <td>${escapeHtml(u.nombre_usuario || "")}</td>
        <td>${escapeHtml(u.nombre_rol || "")}</td>
        ${getRol()==="IT" ? `
        <td>
          <button onclick='editar(${u.id_usuario},"${escapeHtml(u.usuario)}","${escapeHtml(u.nombre_usuario)}",${u.id_permiso_fk || 2})'>Editar</button>
          <button onclick='eliminarU(${u.id_usuario})'>Eliminar</button>
        </td>` : `<td class="it"></td>`}
      `;
      tbody.appendChild(tr);
    });
  }catch(err){
    console.error("[LISTA] ", err);
    msg.textContent = err.message || "Error";
  }
}

function nuevoUsuario(){
  openModal("Nuevo usuario");
  document.getElementById("id_usuario").value = "";
  document.getElementById("usuario").value = "";
  document.getElementById("nombre_usuario").value = "";
  document.getElementById("password").value = "";
  document.getElementById("id_permiso_fk").value = "2";
  document.getElementById("formMsg").textContent = "";
}

function editar(id, usuario, nombre, permiso){
  openModal("Editar usuario");
  document.getElementById("id_usuario").value = id;
  document.getElementById("usuario").value = usuario || "";
  document.getElementById("nombre_usuario").value = nombre || "";
  document.getElementById("password").value = "";
  document.getElementById("id_permiso_fk").value = Number(permiso) || 2;
  document.getElementById("formMsg").textContent = "";
}

function openModal(title){
  document.getElementById("modalTitle").textContent = title;
  const mb = document.getElementById("modalBackdrop");
  mb.style.display = "flex";
  setTimeout(()=> mb.classList.add("show"), 10);
}

function hideModal(){
  const mb = document.getElementById("modalBackdrop");
  mb.classList.remove("show");
  setTimeout(()=> { mb.style.display = "none"; }, 150);
}
function closeModal(e){ if(e.target.id === "modalBackdrop") hideModal(); }

async function guardarUsuario(){
  const formMsg = document.getElementById("formMsg");
  formMsg.textContent = "";

  const id = document.getElementById("id_usuario").value;
  const body = {
    usuario: document.getElementById("usuario").value.trim(),
    nombre_usuario: document.getElementById("nombre_usuario").value.trim(),
    password: (document.getElementById("password").value || undefined),
    id_permiso_fk: Number(document.getElementById("id_permiso_fk").value)
  };

  if(!body.usuario || !body.nombre_usuario) {
    formMsg.textContent = "Usuario y Nombre son requeridos";
    return;
  }

  try{
    if(id){
      await apiSend("PUT", `/user/${id}`, body);
    }else{
      await apiSend("POST", `/user`, body);
    }
    hideModal();
    await cargar();
  }catch(err){
    console.error("[GUARDAR] ", err);
    formMsg.textContent = err.message || "Error";
  }
}

async function eliminarU(id){
  if(!confirm("¿Eliminar usuario?")) return;
  try{
    await apiSend("DELETE", `/user/${id}`);
    await cargar();
  }catch(err){
    alert(err.message || "No se pudo eliminar");
  }
}

function escapeHtml(s){
  return (s ?? "").toString().replace(/[&<>"']/g, c => (
    { "&":"&amp;", "<":"&lt;", ">":"&gt;", '"':"&quot;", "'":"&#39;" }[c]
  ));
}
