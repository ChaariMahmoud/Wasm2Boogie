(module
  ;; Global variables
  (global $counter (mut i32) (i32.const 0))
  (global $pi f32 (f32.const 3.14159))
  (global $max_value (mut i64) (i64.const 1000))

  ;; Table for indirect function calls
  (table 2 funcref)
  (elem (i32.const 0) $add $sub)

  ;; Types
  (type $binary_op (func (param i32 i32) (result i32)))
  (type $unary_op (func (param i32) (result i32)))
  (type $void_op (func))

  ;; Functions
  (func $add (type $binary_op) (param $a i32) (param $b i32) (result i32)
    local.get $a
    local.get $b
    i32.add)

  (func $sub (type $binary_op) (param $a i32) (param $b i32) (result i32)
    local.get $a
    local.get $b
    i32.sub)

  (func $increment_counter (type $void_op)
    global.get $counter
    i32.const 1
    i32.add
    global.set $counter)

  (func $factorial (type $unary_op) (param $n i32) (result i32)
    (local $result i32)
    i32.const 1
    local.set $result
    block $outer
      loop $inner
        local.get $n
        i32.const 1
        i32.lt_s
        br_if $outer
        local.get $result
        local.get $n
        i32.mul
        local.set $result
        local.get $n
        i32.const 1
        i32.sub
        local.set $n
        br $inner
      end
    end
    local.get $result)

  (func $call_indirect (type $binary_op) (param $op i32) (param $a i32) (param $b i32) (result i32)
    local.get $op
    local.get $a
    local.get $b
    call_indirect (type $binary_op))

  (func $memory_ops (type $void_op)
    (local $addr i32)
    (local $value i32)
    i32.const 0
    local.set $addr
    i32.const 42
    local.set $value

    ;; Store and load 32-bit values
    local.get $addr
    local.get $value
    i32.store
    local.get $addr
    i32.load

    ;; Store and load 8-bit values
    local.get $addr
    local.get $value
    i32.store8
    local.get $addr
    i32.load8_u

    ;; Store and load 16-bit values
    local.get $addr
    local.get $value
    i32.store16
    local.get $addr
    i32.load16_u

    drop drop drop drop drop drop)

  (func $float_ops (type $void_op)
    (local $x f32)
    (local $y f64)
    f32.const 1.5
    local.set $x
    f64.const 2.5
    local.set $y

    ;; Basic float operations
    local.get $x
    f32.const 0.5
    f32.add
    local.get $y
    f64.const 0.5
    f64.add
    drop drop)

  ;; Memory with initial size and maximum size
  (memory $mem 1 2)

  ;; Exports
  (export "add" (func $add))
  (export "sub" (func $sub))
  (export "increment_counter" (func $increment_counter))
  (export "factorial" (func $factorial))
  (export "call_indirect" (func $call_indirect))
  (export "memory_ops" (func $memory_ops))
  (export "float_ops" (func $float_ops))
  (export "counter" (global $counter))
  (export "pi" (global $pi))
  (export "max_value" (global $max_value))
  (export "mem" (memory $mem))) 