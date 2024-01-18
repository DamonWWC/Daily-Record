<template>
  <div class="bg-red-400 about mt-100" >
    <h1>This is an about page</h1>
    <button @click="click">click</button>
  </div>
</template>

<script setup>
function click(){
  console.log('click');
  window.chrome.webview.postMessage('hello world');
}

window.chrome.webview.addEventListener('message', (event) => {
  console.log(event);
  alert(event.data);
});
</script>


<style>
/* @media (min-width: 1024px) {
  .about {
    min-height: 100vh;
    display: flex;
    align-items: center;
  }
} */
</style>
