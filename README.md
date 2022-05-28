# Pixel logic
A digital logic simulator made in C#.

You can place components by drawing pixels. These pixels are then converted to components containing connected pixels using a connected components algorithm. after this the components are used to simulate the circuit.


## Preview
<img src="https://github.com/Matts-vdp/pixel-logic/blob/master/preview/count3.png" width="300">

## Controls
| Keys          | Action            |
|---------------|-------------------|
| left click    | place             |
| right click   | remove            |
| scroll wheel  | select component  |
| arrows        | Move Camera       |
| +,-           | Zoom              |
| 0-9           | toggle buttons    |
| S             | save entire grid to saves/saves.json   |
| L             | load entire grid from saves/saves.json |
| hold C + drag | copy selected     |
| hold X + drag | cut selected      |
| hold V        | show clipboard    |
| release V     | place clipboard   |
| I             | import last dragged file as subcomponent (only works with .json files in saves)|



## **How to**
### **Basics**
The main feature of this program is to simulate digital logic. You can use the basic gates to create some cool circuits. 

There are 3 different categories in the basic components.

#### **Wire**
This component can be used to transport signals. Wires are connected if they touch each other so leave a 1 block gap to stop them from connecting.

#### **Gates** 
Applies a logical function to its inputs and puts the result on the output.
Just like wires gates of the same type connect when they touch. This way you can create larger logic gates. For example a 8 bit and. 

#### **Connections** 
Can be used to connect components to each other. There are 4 types.
- **In**: Passes the signal from a connected wire *into* a connected gate.
- **ClkIn**: passes a signal from a connected wire to a connected gate. This acts the same as a **In** except for gates like a flip flop where it acts as the input for the clock signal.
- **Out**: Passes the signal from a connected gate to a connected wire .
- **Cross**: Can be used to cross wires without connecting them.
A typical setup is wire -> in -> gate -> out -> wire.

### **Save and load**
The easiest way to store your creation is to press the **S** key and load it next time with **L**. However You can also save and load parts of your creation. This can e done with the copy and paste functionality.

The last thing you copied is stored in *saves/clipboard.json*. You can rename the file to make sure it isn't overwritten next time you copy something. 

To load a part drag the file into the program window.
The part has to be located in the *saves* folder. 
The part is ready to paste wherever you want.

### **Custom Components**
Custom components are components that you create yourself.
They have to be loaded from a file by dragging it into the program window. 

There are 2 types of custom components

#### **Sub Components**
Sub components are components that combine a circuit into a single component. 

They can be created by dragging a file containing a saved circuit into the window. This has to be a .json file located in *saves*. 
After this press the **I** key. When succesfull the filename should appear as a new option in the component list. 
You can place and use it as normal.

To create a circuit that can be used as a sub component you have to build a circuit and place the needed in and outputs. 

Inputs are created by placing an **Out** connection that is only connected to a wire.
You will see a **I** on the **Out**.

Outputs are created by placing a **In** connection that is only connected to a gate.
You will see a **O** on the **In**.

Clock inputs are created by placing a **ClkIn** connection that is only connected to a gate.
You will see a **C** on the **ClkIn**.

#### **Programmable Components**
Programmable components are components that contain a custom C# script. 
They can be loaded by dragging the correct file into the window. When succesfull a new Option will appear in the component list.

There are 2 types defined by there file extension.
- **.cpl**: Runs every time a input changes.
- **.ppl**: Runs every time the clock input changes from false to true.

Your script has acces to a *List\<bool>* object containing the state of the inputs in variable i. You can set the ouputs by returning a *List\<bool>* object. There are some helper functions and extra variables you have acces to. For more information please look at the **Input** class in *CodeCompilation.cs*.


## Example
For this example we are going to build a basic 3 bit counter. Here is a schematic of a 1 bit module. If you're not sure there is a image of a possible solution at the end.

<img src="https://github.com/Matts-vdp/pixel-logic/blob/master/preview/count-shem.png" width="300">

Let's begin.
### Step 1: Build a 1 bit counter
1. Place a **flip flop** and add a **In**, **Out** and **ClkIn** connection next to it.
2. Place a **Exor** and add 2 **In** and 1 **Out** connection next to it. 
3. Connect the **Out** of the **Exor** to the **In** of the **Flip flop** and the **Out** of the **Flip flop** to the **In** of the **Exor** using **wire**.
4. Place a **And** and add 2 **In** and 1 **Out** connection next to it.
5. Connect 1 **In** to the **Out** of the **Flip flop** and the **other** to the second **In** of the **Exor** using **wire**.
6. Add a **button** with an **Out** connection and connect it to the **wire** of the second **In** of the **Exor**.
7. Add a **Clock** with an **Out** connection and connect it to the **ClkIn** connection of the **Flip flop**.
8. Press the **1** button. You should see the count line toggle.

You have now completed step 1.

If you are not sure about your circuit or you just want to see another example here is my version.

<img src="https://github.com/Matts-vdp/pixel-logic/blob/master/preview/count1.png" width="300">


### Step 2: Expand to 3 bit
Now that we created the 1 bit counter expanding it is very easy.

1. Hold **C** and select the 1 bit counter.
2. Release **C**.
3. Hold **V** and position the new counter where you want. 
4. Release **V** to paste the counter.
5. Connect the **Next** connection to the **Enable** of the next 1 bit counter. These names can be seen in the shematic shown at the start of the example.
6. Repeat 1-5 one more time.
7. Connect the **ClkIn** of all the counters to the **Out** of the **Clock**.
8. Press the **1** button. You should see the 3 bit counter working correctly.

You have now completed step 2.

If you are not sure about your circuit or you just want to see another example here is my version. I have added a display to make reading the count easier.

<img src="https://github.com/Matts-vdp/pixel-logic/blob/master/preview/count3.gif" width="300">
