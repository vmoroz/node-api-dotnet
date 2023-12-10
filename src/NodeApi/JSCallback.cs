// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.JavaScript.NodeApi;

public delegate JSValue JSCallbackFunc(JSCallbackArgs args);
public delegate JSValue JSCallbackFunc0();
public delegate JSValue JSCallbackFunc1(JSValue value1);
public delegate JSValue JSCallbackFunc2(JSValue value1, JSValue value2);
public delegate JSValue JSCallbackFunc3(JSValue value1, JSValue value2, JSValue value3);
public delegate JSValue JSCallbackFunc4(
    JSValue value1, JSValue value2, JSValue value3, JSValue value4);
public delegate JSValue JSCallbackFunc5(
    JSValue value1, JSValue value2, JSValue value3, JSValue value4, JSValue value5);

public delegate void JSCallbackAction(JSCallbackArgs args);
public delegate void JSCallbackAction0();
public delegate void JSCallbackAction1(JSValue value1);
public delegate void JSCallbackAction2(JSValue value1, JSValue value2);
public delegate void JSCallbackAction3(JSValue value1, JSValue value2, JSValue value3);
public delegate void JSCallbackAction4(
    JSValue value1, JSValue value2, JSValue value3, JSValue value4);
public delegate void JSCallbackAction5(
    JSValue value1, JSValue value2, JSValue value3, JSValue value4, JSValue value5);

