# Sistema de Inventario - Guía Completa de Implementación

## Resumen de lo que se implementó

Se ha creado un sistema de inventario completo para "Castle of Self" con:

1. **ItemType.cs** - Enum con 5 tipos de items:
   - KeySilver (Llave Plateada)
   - KeyGold (Llave Dorada)
   - SpellSlow (Hechizo Ralentización)
   - SpellFreeze (Hechizo Congelación)
   - SpellClear (Hechizo Eliminar)

2. **ItemStack.cs** - Clase que representa un grupo de items del mismo tipo con cantidad:
   - Propiedades: itemType, quantity
   - Método ToString() para debug

3. **Inventory.cs** - Sistema de almacenamiento de items expandido:
   - Lista de ItemStack (máximo 5 tipos distintos)
   - AddItem(ItemType, quantity) - Añade items con acumulación automática
   - RemoveItem(ItemType, quantity) - Quita items
   - HasItem(ItemType, minQuantity) - Verifica disponibilidad
   - GetQuantity(ItemType) - Obtiene cantidad
   - Clear() - Vacía el inventario
   - GetDebugInfo() - Retorna string formateado del inventario

4. **ItemPickup.cs** - Componente para items sueltos en el mundo:
   - Puede ser recolectado con la tecla E
   - Se configura automáticamente con tag "Item" y layer "RayCastDetect"
   - Llama a Inventory.AddItem() cuando se recoge
   - Se destruye después de ser recogido

5. **ChestController.cs** - Actualizado con sistema de contenido:
   - Lista de ItemStack que contiene el cofre
   - Contenido de prueba por defecto (1 KeySilver + 2 SpellSlow)
   - Método TransferItemsToPlayer() para pasar items al inventario
   - Método GetContents() para inspeccionar contenido

6. **Selected.cs** - Actualizado para detectar items:
   - Añadida detección de tag "Item"
   - Llama a ItemPickup.PickUp() cuando se presiona E sobre un item

7. **InventoryUI.cs** - UI de debug para mostrar inventario en pantalla:
   - Actualiza constantemente en pantalla
   - Muestra cada tipo de item y su cantidad

---

## Instrucciones de Configuración Paso a Paso

### PASO 1: Crear Tag "Item"
1. En la carpeta raíz del proyecto (Assets/), selecciona el Player prefab o cualquier GameObject
2. En el Inspector, arriba a la derecha verás "Tag" dropdown (por defecto "Untagged")
3. Haz clic en el dropdown y selecciona "Add Tag"
4. Añade un nuevo tag llamado **"Item"** (exactamente así)
5. Guarda el proyecto

### PASO 2: Verificar Layer "RayCastDetect"
1. En el menú superior: Window > Layers
2. Verifica que exista el layer **"RayCastDetect"**
3. Si no existe, crea uno en los slots disponibles
4. Cierra la ventana de Layers

### PASO 3: Crear ItemPickup Prefab de Prueba
1. En la escena, crea un nuevo GameObject: Click derecho en Hierarchy > 3D Object > Cube
2. Renómbralo a "ItemPickup_Test"
3. En el Inspector:
   - Añade componente **ItemPickup** (Add Component > ItemPickup)
   - Configura:
     - Item Type: **KeySilver**
     - Quantity: **1**
4. Configura el Collider:
   - Asegúrate que tiene un BoxCollider
   - Marca Is Trigger = **true** (importante para que se pueda recoger)
5. Configura el GameObject:
   - Tag: **Item** (en el dropdown superior derecho)
   - Layer: **RayCastDetect**
6. Posiciona en la escena donde pueda alcanzar el jugador (ej: (0, 1, 5))
7. Guarda como prefab: Arrastra el GameObject a una carpeta (Assets/Prefabs/) y confirma

### PASO 4: Actualizar Player con Inventory
1. Selecciona el Player GameObject
2. En el Inspector:
   - Verifica que tenga un componente **Inventory** (si no, Add Component > Inventory)
   - Max Slots: **5** (no toques, es el por defecto)
3. Guarda la escena

### PASO 5: Crear Canvas de UI para InventoryUI
1. En la Hierarchy, click derecho > UI > Panel (Canvas)
2. Se creará automáticamente un Canvas
3. En el Canvas, crea un Text:
   - Click derecho en Canvas > UI > Text (TextMeshPro)
   - Si te pide importar TextMeshPro, haz click en "Import TextMeshPro Essentials"
4. Renómbrala a "InventoryText"
5. Configura el Layout:
   - Selecciona InventoryText en Hierarchy
   - En el Inspector, configura:
     - Pos X: 0, Pos Y: 0
     - Width: 300, Height: 400
     - Alignment: Top Left
     - Text content: "Inventory"
6. Crea un nuevo GameObject vacío en el Canvas:
   - Click derecho en Canvas > Create Empty
   - Renómbralo a "InventoryUIManager"
7. Selecciona InventoryUIManager y añade componente **InventoryUI**:
   - Player Movement: Arrastra el Player desde Hierarchy
   - Inventory Text: Arrastra InventoryText desde Hierarchy
8. Guarda la escena

### PASO 6: Probar Sistema
1. Presiona Play en el editor
2. Acércate al ItemPickup_Test con el Player
3. Presiona E para recoger el item
4. Verifica que:
   - El item desaparece de la pantalla
   - El texto en Canvas muestra "Inventory: KeySilver x1"
   - Console muestra logs de "ItemPickup: KeySilver x1 recogido"
5. Crea otro ItemPickup del mismo tipo y recógelo para verificar acumulación:
   - Debería mostrar "KeySilver x2"

### PASO 7: Conectar Cofres con Inventario (Opcional - Mejora Futura)
Si quieres que los cofres transfieran items automáticamente cuando se abren:
1. Selecciona el ChestController en la escena
2. En el método AbrirCofre() o después de la animación, llama:
   ```
   TransferItemsToPlayer(player);
   ```
   (Esto requiere pasar el Player como parámetro)

---

## Flujo de Funcionamiento

### Cuando el jugador presiona E sobre un Item:
1. **Selected.cs** detecta hit con tag "Item"
2. Obtiene el componente **ItemPickup**
3. Llama a `itemPickup.PickUp(player)`
4. **ItemPickup.PickUp()** obtiene el componente Inventory del jugador
5. Llama a `inventory.AddItem(itemType, quantity)`
6. **Inventory.AddItem()** busca si ya existe ese item:
   - Si existe: suma la cantidad al stack existente
   - Si no existe y hay espacio: crea nuevo stack
   - Si no hay espacio: retorna false
7. Si fue exitoso, **ItemPickup** se destruye
8. **InventoryUI** detecta el cambio y actualiza el texto en pantalla

### Contenido de Cofres:
- Se define en la lista `contents` del ChestController
- Se configura por defecto en Start() con 1 KeySilver y 2 SpellSlow
- El método `TransferItemsToPlayer()` puede transferir todo al inventario cuando el jugador lo desee

---

## Scripts Creados/Modificados

| Archivo | Acción | Ubicación |
|---------|--------|-----------|
| ItemType.cs | CREADO | Assets/Scripts/PLayer/ |
| ItemStack.cs | CREADO | Assets/Scripts/PLayer/ |
| Inventory.cs | CREADO | Assets/Scripts/PLayer/ |
| ItemPickup.cs | CREADO | Assets/Scripts/PLayer/ |
| ChestController.cs | MODIFICADO | Assets/Scripts/PLayer/ |
| Selected.cs | MODIFICADO | Assets/Scripts/PLayer/ |
| InventoryUI.cs | CREADO | Assets/Scripts/PLayer/ |

---

## Debugging y Logs

Al recoger items, verás en Console:
```
Inventory: KeySilver x1 añadido. Total stacks: 1/5
ItemPickup: KeySilver x1 recogido.
```

Si intentas recoger un item pero no hay espacio:
```
Inventory: no hay espacio para más tipos de items. Max slots: 5
ItemPickup: no se pudo añadir KeySilver al inventario.
```

---

## Próximos Pasos (Futuro)

1. Crear más ItemPickup prefabs con otros tipos
2. Integrar sistema de uso de items (consumir SpellSlow, etc.)
3. Crear UI visual con iconos en lugar de texto
4. Sincronizar inventario con guardado de juego
5. Añadir sonidos al recoger items
6. Sistema de crafteo con items

---

## Notas Importantes

- **ItemPickup destruye el GameObject automáticamente** después de ser recogido
- **El inventario está limitado a 5 tipos diferentes**, pero cada tipo puede tener cantidad ilimitada
- **Los items deben tener el tag "Item"** para ser detectados por Selected.cs
- **Los items deben estar en el layer "RayCastDetect"** para que el raycast los detecte
- **InventoryUI actualiza cada frame**, por lo que los cambios se ven inmediatamente
