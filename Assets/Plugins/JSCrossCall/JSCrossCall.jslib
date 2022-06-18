//hrmm... need to figure out HOW this bit is integrated

debugger;

//I think we need to create a "JSCrossCall" object, and then mergeInto function references. Not sure if we can use IIFE or similar here
//see https://emscripten.org/docs/porting/connecting_cpp_and_javascript/Interacting-with-code.html it's weird
//also see https://answers.unity.com/questions/1368055/webgl-build-jspre-javascript-plugin-how-do-i-call.html
//https://medium.com/@depanther/unity-webgl-secrets-b6ddf214f1fd
//not 100% sure but I think almost everything needs to be in a jspre, not jslib


////if we want to slap things onto the Module object, var Module should be in scope
//we are in the scope of the unityFramework() function, which is what I really wanted to know
//so anything we declare in jspre we can reference here

mergeInto(LibraryManager.library, {

	//this absolutely needs to be here
	CSCallThunk: function (strPtr) {
		const dataStr = Pointer_stringify(str);
		const data = JSON.parse(dataStr);
		debugger;

		//TODO execute call
	},

	//this should be elsewhere I think
	//I think this doesn't even get emitted because it's not referenced from the C (read: Unity) world
	callCS: function (target, args) {
		const obj = {
			target: target,
			args: args
		};
		const str = JSON.stringify(obj);
		unityInstance.SendMessage('CCMonoBehaviourHook', 'JSCallThunk', str);
	},

});

//public functions (JS->CS)
/*
function pushBroadcastMessage(flag, values) {
	
}

function callScript (script, ...args) {
	
}
*/