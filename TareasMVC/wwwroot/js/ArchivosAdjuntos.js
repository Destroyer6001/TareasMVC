let inputArchivoTarea = document.getElementById('archivoATarea');

function manejarClickAgregarArchivoAdjuntos() {
    inputArchivoTarea.click();
}

async function manejarSeleccionArchivoTarea(event) {
    const archivos = event.target.files;
    const archvivosArreglo = Array.from(archivos);

    const formData = new FormData();
    const idtarea = tareaEditarVm.id;
    for (var i = 0; i < archvivosArreglo.length; i++) {
        formData.append("archivos", archvivosArreglo[i]);
    }

    const respuesta = await fetch(`${urlArchivos}/${idtarea}`, {
        body = formData,
        method = "POST"
    });

    if (!respuesta) {
        manejarErrorApi(respuesta);
        return;
    }

    const json = await respuesta.json();
    prepararArchivosAdjuntosR(json);

    inputArchivoTarea.value = null;
}

function prepararArchivosAdjuntos(archivosAdjuntos) {
    archivosAdjuntos.foreach(archivoAdjunto => {
        let fechaCreacion = archivoAdjunto.fechaCreacion;
        if (archivoAdjunto.fechaCreacion.indexOf('Z') === - 1) {
            fechaCreacion += 'Z';
        }

        const fechaCreactionDT = new Date(fechaCreacion);
        archivoAdjunto.publicado = fechaCreactionDT.toLocaleString();
        tareaEditarVM.archivosAdjuntos.push(new archivoAdjuntoViewModel({
            ...archivoAdjunto, modoEdicion: false
        }));
    });
    let tituloArchivoAdjunto;
    function manejarClickTituloArchivoAdjunto(archivoAdjunto) {
        archivoAdjunto.modoEdicion(true);
        tituloArchivoAdjunto = archivoAdjunto.titulo();
        $("[name = 'txtArchivoAdjuntoTitulo']: visible").focus();
    }

    async function manejarFocusoutTituloArchivoAdjunto(archivoAdjunto) {
        archivoAdjunto.modoEdicion(false);
        const idTarea = archivoAdjunto.id;

        if (!archivoAdjunto.titulo()) {
            archivoAdjunto.titulo(tituloArchivoAdjunto);
        }
        if (archivoAdjunto.titulo === tituloArchivoAdjunto) {
            return;
        }

        const data = JSON.stringify(archivoAdjunto.titulo());
        const respuesta = await fetch(`${urlArchivos}/${idTarea}`, {
            body: data,
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            }
        });
        if (!respuesta.ok) {
            manejarErrorApi(respuesta);
        }
    }
}

function manejarClickBorrarArchivoAdjunto(archivoAdjunto) {
    modalEditarTareaBootstrap.hide();

    confirmarAccion({
        callbackAceptar: () => {
            borrarArchivoAdjunto(archivoAdjunto);
            modalEditarTareaBootstrap.show();
        },

        callbackCancelar: () => {
            modalEditarTareaBootstrap.show();
        },
        titulo: '¿Desea borrar este archivo adjunto?'
    });
}

async function borrarArchivoAdjunto(archivoAdjunto) {
    const respuesta = await fetch(`${urlArchivos}/${archivoAdjunto.id}`, {
        method: 'DELETE',
    });

    if (!respuesta.ok) {
        manejarErrorApi(respuesta);
        return;
    }

    tareaEditarVM.archivosAdjuntos.remove(function (item) { return item.id == archivoAdjunto.id });
}

function manejarClickDescargarArchivoAdjunto(archivoAdjunto) {
    descargarArchivos(archivoAdjunto.url, archivoAdjunto.titulo);
}