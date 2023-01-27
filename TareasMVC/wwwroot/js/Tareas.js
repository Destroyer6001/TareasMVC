function agregarNuevaTareaAlListado(){
    tareaListadoViewModel.tareas.push(new tareaElementoListadoViewModel({ id: 0, titulo: '' }));

    $("[name=titulo-tarea]").last().focus();
}

async function manejarFocusoutTituloTarea(tarea) {
    const titulo = tarea.titulo();
    if (!titulo) {
        tareaListadoViewModel.tareas.pop();
        return;
    }
    const data = JSON.stringify(titulo);
    const respuesta = await fetch(urlTareas, {
        method: 'POST',
        body: data,
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (respuesta.ok) {
        const json = await respuesta.json();
        tarea.id(json.id);
    } else {
        manejarErrorApi(respuesta);
    }
}

async function ObtenerTareas() {
    tareaListadoViewModel.cargando(true);

    const respuesta = await fetch(urlTareas, {
        method: 'GET',
        headers:
        {
            'Content-Type': 'application/json'
        }
    });

    if (!respuesta.ok) {
        manejarErrorApi(respuesta);
        return;
    }

    const json = await respuesta.json();
    tareaListadoViewModel.tareas([]);

    json.forEach(valor => {
        tareaListadoViewModel.tareas.push(new tareaElementoListadoViewModel(valor));
    })

    tareaListadoViewModel.cargando(false);
}

async function actualizarOrdenTareas() {
    const ids = obtenerIdsTareas();
    await enviarIdsTareasBackend(ids);

    const arregloOrdenado = tareaListadoViewModel.tareas.sorted(function (a, b) {
        return ids.indexOf(a.id().toString()) - ids.indexOf(b.id().toString());
    });

    tareaListadoViewModel.tareas([]);
    tareaListadoViewModel.tareas(arregloOrdenado);
}

async function enviarIdsTareasBackend(ids) {
    var data = JSON.stringify(ids);
    await fetch(`${urlTareas}/ordenar`, {
        method: 'POST',
        body: data,
        headers: {
            'Content-Type': 'application/json'
        }
    });
}

function obtenerIdsTareas() {
    const ids = $("[name=titulo-tarea]").map(function () {
        return $(this).attr("data-id");
    }).get();
    return ids
}

$(function () {
    $("#reordenable").soportable({
        axis: 'y',
        stop: async function () {
            await actualizarOrdenTareas();
        }
    })
})

async function manejarClickTarea(tarea) {
    if (tarea.esNuevo()) {
        return;
    }

    const Respuesta = await fetch(`${urlTareas}/${tarea.id()}`, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/jsom'
        }
    });

    if (!Respuesta.ok) {
        manejarErrorApi(Respuesta);
        return;
    }

    const json = await Respuesta.json();

    tareaEditarVM.id = json.id;
    tareaEditarVM.titulo(json.titulo);
    tareaEditarVM.descripcion(json.descripcion);
    tareaEditarVM.pasos([]);
    json.pasos.forEach(paso => {
        tareaEditarVM.pasos.push(new pasoViewModel({...paso, modoEdicion: false}))
    })

    prepararArchivosAdjuntos(json.archivosAdjuntos);
    modalEditarTareaBootstrap.show();
}

    async function manejarCambioEditarTarea() {
        const obj = {
            id: tareaEditarVM.id,
            titulo: tareaEditarVM.titulo(),
            descripcion: tareaEditarVM.descripcion(),
        };

        if (obj.titulo) {
            return;
        }

        await editarTareaCompleta(obj);

        const indice = tareaListadoViewModel.tareas().findIndex(t => t.id() === obj.id);
        const tarea = tareaListadoViewModel.tareas()[indice];
        tarea.titulo(obj.titulo);

    }

    async function editarTareaCompleta(tarea) {
        const data = JSON.stringify(tarea);

        const respuesta = await fetch(`${urlTareas}/${tarea.id}`, {
            method: 'PUT',
            body: data,
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (!respuesta.ok) {
            manejarErrorApi(respuesta);
            throw error;
        }
    }
    async function BorrarTarea(tarea) {
        const idtarea = tarea.id;
        const respuesta = await fetch(`${urlTareas}/${idtarea}`, {
            method: 'DELETE',
            headers:
            {
                'Content-Type' : 'application/json'
            }
        });

        if (respuesta.ok) {
            const indice = obtenerIndiceTareaEnEdicion();
            tareaListadoViewModel.tareas.splice(indice, 1);
        }
    }

    function ObtenerIndiceTareaEnEdicion() {
        return tareaListadoViewModel.tareas().findIndex(t => t.id() == tareaEditarVM.id);
    }


    function intentarBorrarTarea(tarea) {
        modalEditarTareaBootstrap.hide();

        confirmarAccion({
            callbackAceptar: () => {
                BorrarTarea(tarea);
            },
            callbackCancelar: () => {
                modalEditarTareaBootstrap.show();
            },
            titulo: `¿Desea borrar la tarea ${tarea.titulo()}?`
        });
}

function ObtenerTareaEnEdicion() {
    const indice = obtenerIndiceTareaEnEdicion();
    tareaListadoViewModel.tareas()[indice];
}
