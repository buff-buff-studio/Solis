function load()
    print("load")
end

function update()
    --Door
    v = getBool("Button0") or getBool("Button1")
    setBool("Door", v)
    
    --Slider
    setFloat("Output", getFloat("Slider") * 9)
end

function unload()
    print("unload")
end