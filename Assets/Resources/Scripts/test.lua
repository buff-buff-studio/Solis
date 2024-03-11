function load()
    print("load")
end

function update()
    --Door
    v = getBool("Button0") and getBool("Button1")
    setBool("Door", v)
    
    --Slider
    setFloat("Output", getFloat("Slider") + 1)
end

function unload()
    print("unload")
end