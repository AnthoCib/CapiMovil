document.addEventListener('DOMContentLoaded', () => {
  const input = document.getElementById('passwordInput');
  const btn = document.getElementById('togglePassword');
  if (!input || !btn) return;

  btn.addEventListener('click', () => {
    const show = input.type === 'password';
    input.type = show ? 'text' : 'password';
    btn.innerHTML = show ? '<i class="bi bi-eye-slash"></i>' : '<i class="bi bi-eye"></i>';
  });
});
