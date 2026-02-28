section .data
    msg db "SHADOW WIZARD MONEY GANG WAS HERE", 0xA
    len equ $ - msg
    revenge db "but we're getting our money back", 0xA
    rev_len equ $ - revenge

section .text
    global _start

_start:
    ; write shadow wizard message
    mov rax, 1
    mov rdi, 1
    mov rsi, msg
    mov rdx, len
    syscall

    ; write revenge message
    mov rax, 1
    mov rdi, 1
    mov rsi, revenge
    mov rdx, rev_len
    syscall

    ; exit (with style)
    mov rax, 60
    xor rdi, rdi
    syscall
