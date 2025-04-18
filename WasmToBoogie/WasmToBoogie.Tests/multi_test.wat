(module
  ;; Test 1: Simple addition with named parameters
  (func $add (param $a i32) (param $b i32) (result i32)
    local.get $a
    local.get $b
    i32.add)
  
  ;; Test 2: Function with local variables
  (func $sum (param $n i32) (result i32)
    (local $sum i32)
    (local $i i32)
    i32.const 0
    local.set $sum
    i32.const 0
    local.set $i
    (loop $loop
      local.get $i
      local.get $n
      i32.lt_s
      if
        local.get $sum
        local.get $i
        i32.add
        local.set $sum
        local.get $i
        i32.const 1
        i32.add
        local.set $i
        br $loop
      end
    )
    local.get $sum)
  
  ;; Test 3: Function with memory operations
  (func $store_and_load (param $offset i32) (param $value i32) (result i32)
    local.get $offset
    local.get $value
    i32.store
    local.get $offset
    i32.load)
  
  ;; Test 4: Function with multiple results
  (func $divmod (param $a i32) (param $b i32) (result i32 i32)
    local.get $a
    local.get $b
    i32.div_s
    local.get $a
    local.get $b
    i32.rem_s)
  
  (memory 1)
  (export "add" (func $add))
  (export "sum" (func $sum))
  (export "store_and_load" (func $store_and_load))
  (export "divmod" (func $divmod))
) 